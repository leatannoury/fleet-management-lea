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

namespace FleetService.Infrastructure.Messaging.Consumers;

public class TripCompletedConsumer : BackgroundService 
//extends background service, means it runs in the background as a seperate thread
//from the the api boots up
{
    private readonly IServiceProvider _serviceProvider;

    public TripCompletedConsumer(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        var connection = factory.CreateConnection();
        var channel = connection.CreateModel();

        channel.QueueDeclare("trip.completed", false, false, false, null);

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += async (model, ea) =>
        {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
            var message = JsonSerializer.Deserialize<TripCompletedEvent>(json);

            // first create the scope
            using var scope = _serviceProvider.CreateScope();

            // then fetch the services
            var vehicleService = scope.ServiceProvider.GetRequiredService<VehicleService>();
            var driverService = scope.ServiceProvider.GetRequiredService<DriverService>();

            // then update the entities
            await vehicleService.UpdateMileage(message.VehicleId, message.DistanceKm);
            
            
            await driverService.MarkDriverAvailable(message.DriverId); 
        };

        //the fleet consumes tripcompleted 
        channel.BasicConsume("trip.completed", true, consumer);

        return Task.CompletedTask;
    }
}