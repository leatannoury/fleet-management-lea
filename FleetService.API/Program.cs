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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<FleetDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("FleetDb")));//configure fleet dbcontext

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


app.UseHttpsRedirection();

app.MapControllers();

app.Run();
