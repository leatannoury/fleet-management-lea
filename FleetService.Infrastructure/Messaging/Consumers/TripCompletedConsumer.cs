//fleet listens to trip events
//listens to trip completions

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using SharedKernel.Events;
using Microsoft.Extensions.DependencyInjection;
using FleetService.Application.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace FleetService.Infrastructure.Messaging.Consumers;

public class TripCompletedConsumer : BackgroundService
//extends background service, means it runs in the background as a seperate thread
//from the the api boots up
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TripCompletedConsumer> _logger;
    private readonly string _rabbitHost;


    public TripCompletedConsumer(IServiceProvider serviceProvider, ILogger<TripCompletedConsumer> logger, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _rabbitHost = configuration["RabbitMQ:Host"] ?? "localhost";
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory() { HostName = _rabbitHost };
        var connection = factory.CreateConnection();
        var channel = connection.CreateModel();

        channel.QueueDeclare("trip.completed", false, false, false, null);

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += async (model, ea) =>
        {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
            var message = JsonSerializer.Deserialize<TripCompletedEvent>(json);

            //log that consumer received the event
            _logger.LogInformation("[CONSUMER] trip.completed received — TripId: {TripId}, VehicleId: {VehicleId}, Distance: {DistanceKm} km",
                message?.TripId, message?.VehicleId, message?.DistanceKm);

             // first create the scope
            using var scope = _serviceProvider.CreateScope();

            // then fetch the services
            var vehicleService = scope.ServiceProvider.GetRequiredService<VehicleService>();
            var driverService = scope.ServiceProvider.GetRequiredService<DriverService>();

            // then update the entities
            await vehicleService.UpdateMileage(message.VehicleId, message.DistanceKm);
            await driverService.MarkDriverAvailable(message.DriverId);

            //log that trip completed
            _logger.LogInformation("[CONSUMER] trip.completed processed — mileage updated, driver marked Available");
        };

        //the fleet consumes tripcompleted 
        channel.BasicConsume("trip.completed", true, consumer);

        return Task.CompletedTask;
    }
}