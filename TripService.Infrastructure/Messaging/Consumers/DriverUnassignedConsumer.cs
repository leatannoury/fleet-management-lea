//trip consumes fleet events (local cache)
//trip updates its local cache when drivers are assigned
// trip service should not directly access fleet db
// trip service needs to know if a driver is available before assigning to a trip. 
// But it can't just query fleet db. so the solution is:
// when fleet unassigns a driver, it publishes "driver.unassigned" to RabbitMQ.
// Trip Service's DriverUnassignedConsumer (running in background) receives that message
// It updates the in-memory RAM cache (IMemoryCache) with the driver's status
// When AssignTripAsync() is called, it checks the cache FIRST (before even touching the domain)


using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using SharedKernel.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace TripService.Infrastructure.Messaging.Consumers;

public class DriverUnassignedConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DriverUnassignedConsumer> _logger;
    private readonly string _rabbitHost;


    public DriverUnassignedConsumer(IServiceProvider serviceProvider, ILogger<DriverUnassignedConsumer> logger, IConfiguration configuration)
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

        channel.QueueDeclare("driver.unassigned", false, false, false, null);

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += (model, ea) =>
        {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
            var message = JsonSerializer.Deserialize<DriverUnassignedEvent>(json);

            //log that consumer received the event
            _logger.LogInformation("[CONSUMER] driver.unassigned received — DriverId: {DriverId}, VehicleId: {VehicleId}",
                message?.DriverId, message?.VehicleId);

            using var scope = _serviceProvider.CreateScope();
            var cache = scope.ServiceProvider.GetRequiredService<IMemoryCache>();

            // Driver handles themselves but unassigned from vehicle
            cache.Remove($"VehicleStatus_{message.VehicleId}");
        };

        channel.BasicConsume("driver.unassigned", true, consumer);
        
        return Task.CompletedTask;
    }
}
