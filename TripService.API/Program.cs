using Microsoft.EntityFrameworkCore;
using SharedKernel.Middleware;
using TripService.Infrastructure.Persistence;
using TripService.Infrastructure.Repositories;
using TripService.Application.Interfaces;
using TripService.Application.Services;
using TripService.Infrastructure.Messaging.Consumers;
using SharedKernel.Messaging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using RabbitMQ.Client;



//configure serilog
//set up the logger so it write structured json logs to the console
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information() //only log information level and above (not debug/verbose)
    .WriteTo.Console(outputTemplate: 
        "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}") //format of the log 
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog(); //tell .net to use serilog as its logger

//add services to the container


builder.Services.AddDbContext<TripDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))); //register trip db context

//register repositories
builder.Services.AddScoped<ITripRepository, TripRepository>();
builder.Services.AddScoped<ITripLogRepository, TripLogRepository>();


//register services
builder.Services.AddScoped<TripService.Application.Services.TripService>();
builder.Services.AddScoped<TripTrackingService>();

//register consumer
builder.Services.AddHostedService<DriverAssignedConsumer>();
builder.Services.AddHostedService<DriverUnassignedConsumer>();
builder.Services.AddHostedService<DriverStatusChangedConsumer>();
builder.Services.AddHostedService<VehicleStatusChangedConsumer>();


builder.Services.AddScoped<TripService.Application.Interfaces.ITripPricingStrategy, TripService.Application.Services.Strategies.StandardPricingStrategy>();

//keycloak jwt authentication setup
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // tell the api where keycloak lives and which realm to work
        options.Authority = builder.Configuration["Keycloak:Authority"] ?? "http://localhost:8080/realms/fleet-platform";
        
        //disable https requirement (we are now localhost for development
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false, // Keycloak doesn't always include the audience, so we disable it for dev
            ValidateIssuer = false,
            ValidIssuer = builder.Configuration["Keycloak:Authority"] ?? "http://localhost:8080/realms/fleet-platform",
        };

        // Keycloak nests roles inside a complex 'realm_access' JSON object. ASP.NET cannot read this natively.
        // This event interceptor captures the token, read the json dictionary, and maps them manually into standard .NET Claims.
        options.Events = new JwtBearerEvents
        {
            // by default, standard JWT authentication relies on passing authorization: 
            // bearer <token> in the http headers. 
            // but standard WebSockets which SignalR uses do not support http headers 
            // natively in typical browser apps. 
            // to fix this, we allow the token to be passed in the URL 
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                //if the request is pointing to our signal r hub, then extract the token from the url
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/trip-tracking"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            },

            OnTokenValidated = context =>
            {
                if (context.Principal?.Identity is System.Security.Claims.ClaimsIdentity identity)
                {
                    var realmAccessClaim = identity.FindFirst("realm_access");
                    if (realmAccessClaim != null)
                    {
                        var realmAccessAsDict = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, string[]>>(realmAccessClaim.Value);
                        if (realmAccessAsDict != null && realmAccessAsDict.TryGetValue("roles", out var roles))
                        {
                            foreach (var role in roles)
                            {
                                identity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, role));
                            }
                        }
                    }
                }
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddSignalR();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// read RabbitMQ host from config (overridden by docker-compose env var)
var rabbitHost = builder.Configuration["RabbitMQ:Host"] ?? "localhost";

//register health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(
        builder.Configuration.GetConnectionString("DefaultConnection")!, 
        name: "postgresql",
        tags: new[] { "db", "trip" }
    )
    .AddRabbitMQ(rabbitConnectionString: $"amqp://guest:guest@{rabbitHost}:5672", // test rabbitmq connection
        name: "rabbitmq",
        tags: new[] { "messaging" }
    );

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer {your token}'"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

//publish messages
builder.Services.AddSingleton<IEventPublisher>(new RabbitMqPublisher(rabbitHost));



builder.Services.AddMemoryCache();//register memory cache



var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>(); //register global exception middleware

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication(); 
app.UseAuthorization();
app.MapControllers();
app.MapHub<TripService.API.Hubs.TripTrackingHub>("/hubs/trip-tracking");//map the hub
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description
            })
        });
        await context.Response.WriteAsync(result);
    }
}); // expose /health with detailed json output


// Auto-apply EF Core migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TripDbContext>(); // use TripDbContext here
    db.Database.Migrate();
}

app.Run();