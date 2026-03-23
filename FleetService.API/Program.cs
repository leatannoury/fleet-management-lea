using FleetService.Application.Interfaces;
using FleetService.Application.Services;
using FleetService.Infrastructure.Messaging;
using FleetService.Infrastructure.Messaging.Consumers;
using FleetService.Infrastructure.Persistence;
using FleetService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Middleware;
using TripService.Application.Interfaces;
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

//keycloak jwt authentication setup
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // tell the api where keycloak lives and which realm to work
        options.Authority = "http://localhost:8080/realms/fleet-platform";
        
        //disable https requirement (we are now localhost for development
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false, // Keycloak doesn't always include the audience, so we disable it for dev
            ValidateIssuer = true,
            ValidIssuer = "http://localhost:8080/realms/fleet-platform",
        };

        // Keycloak nests roles inside a complex 'realm_access' JSON object. ASP.NET cannot read this natively.
        // This event interceptor captures the token, read the json dictionary, and maps them manually into standard .NET Claims.
        options.Events = new JwtBearerEvents
        {
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

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

//register health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(
        builder.Configuration.GetConnectionString("DefaultConnection")!, // test the postgres connection
        name: "postgresql",
        tags: new[] { "db", "fleet" }
    )
    .AddRabbitMQ(rabbitConnectionString: "amqp://guest:guest@localhost:5672", // test rabbitmq connection (port 5672 = AMQP, NOT 15672 which is the web UI)
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

builder.Services.AddDbContext<FleetDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));//configure fleet dbcontext

//register the repositories
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IDriverRepository, DriverRepository>();

//register services
builder.Services.AddScoped<VehicleService>();
builder.Services.AddScoped<DriverService>();

//register rabbitmq event publisher
builder.Services.AddSingleton<IEventPublisher, RabbitMqPublisher>();

//register consumer
builder.Services.AddHostedService<TripCompletedConsumer>();
builder.Services.AddHostedService<TripStartedConsumer>();


var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>(); //register global exception middleware



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseAuthentication(); 
app.UseAuthorization();
app.MapControllers();
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


app.Run();
