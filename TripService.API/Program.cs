using Microsoft.EntityFrameworkCore;
using SharedKernel.Middleware;
using TripService.Infrastructure.Persistence;
using TripService.Infrastructure.Repositories;
using TripService.Application.Interfaces;
using TripService.Application.Services;
using TripService.Infrastructure.Messaging.Consumers;
using SharedKernel.Messaging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

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

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//publish messages
builder.Services.AddSingleton<IEventPublisher, RabbitMqPublisher>();


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

app.UseAuthorization();

app.MapControllers();

app.Run();