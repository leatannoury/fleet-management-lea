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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace FleetService.Infrastructure.Messaging.Consumers;

public class TripStartedConsumer : BackgroundService
//extends background service, means it runs in the background as a seperate thread
//from the the api boots up
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TripStartedConsumer> _logger;
    private readonly string _rabbitHost;


    public TripStartedConsumer(IServiceProvider serviceProvider, ILogger<TripStartedConsumer> logger, IConfiguration configuration)
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

        channel.QueueDeclare("trip.started", false, false, false, null);

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += async (model, ea) =>
        {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
            var message = JsonSerializer.Deserialize<TripStartedEvent>(json);

            //log that consumer received the event
            _logger.LogInformation("[CONSUMER] trip.started received — TripId: {TripId}, DriverId: {DriverId}",
                message?.TripId, message?.DriverId);

            //create scope
            using var scope = _serviceProvider.CreateScope();
            //fetch the services
            var driverService = scope.ServiceProvider.GetRequiredService<DriverService>();
            //update entities
            await driverService.MarkDriverOnTrip(message.DriverId);

            //log that consumer processed trip started
            _logger.LogInformation("[CONSUMER] trip.started processed — Driver {DriverId} marked as OnTrip", message?.DriverId);
        };

        //fleet consumes tripstarted
        channel.BasicConsume("trip.started", true, consumer);

        return Task.CompletedTask;
    }
}
