//fleet listens to trip starts
//when a trip starts, fleet needs to know the driver is now busy

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using SharedKernel.Events;
using FleetService.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FleetService.Infrastructure.Messaging.Consumers;

public class TripStartedConsumer : BackgroundService
//extends background service, means it runs in the background as a seperate thread
//from the the api boots up
{
    private readonly IServiceProvider _serviceProvider;

    public TripStartedConsumer(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        var connection = factory.CreateConnection();
        var channel = connection.CreateModel();

        channel.QueueDeclare("trip.started", false, false, false, null);

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += async (model, ea) =>
        {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
            var message = JsonSerializer.Deserialize<TripStartedEvent>(json);

                //create scope
            using var scope = _serviceProvider.CreateScope();
            //fetch the services
            var driverService = scope.ServiceProvider.GetRequiredService<DriverService>();
            //update entities
            await driverService.MarkDriverOnTrip(message.DriverId);
        };

        //fleet consumes tripstarted
        channel.BasicConsume("trip.started", true, consumer);

        return Task.CompletedTask;
    }
}
