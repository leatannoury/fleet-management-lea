//trip consumes fleet events (local cache)
//trip updates its local cache when status of vehicle changes
// trip service should not directly access fleet db
// trip service needs to know vehicle status  before assigning to a trip. 
// But it can't just query fleet db. so the solution is:
// when fleet changes a vehicle status, it publishes "vehicle.status.changed" to RabbitMQ.
// Trip Service's Vehicle Status Changed Consumer (running in background) receives that message
// It updates the in-memory RAM cache (IMemoryCache) with the vehicle's status
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

public class VehicleStatusChangedConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<VehicleStatusChangedConsumer> _logger;
    private readonly string _rabbitHost;


    public VehicleStatusChangedConsumer(IServiceProvider serviceProvider, ILogger<VehicleStatusChangedConsumer> logger, IConfiguration configuration)
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

        channel.QueueDeclare("vehicle.status.changed", false, false, false, null);

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += (model, ea) =>
        {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
            var message = JsonSerializer.Deserialize<VehicleStatusChangedEvent>(json);

            //log that consumer received the event
            _logger.LogInformation("[CONSUMER] vehicle.status.changed received — VehicleId: {VehicleId}, Status: {Status}, Type: {Type}",
                message?.VehicleId, message?.Status, message?.VehicleType);

            using var scope = _serviceProvider.CreateScope();
            var cache = scope.ServiceProvider.GetRequiredService<IMemoryCache>();

            cache.Set($"VehicleStatus_{message.VehicleId}", message.Status);

            // [NEW] Cache the vehicle type for pricing calculations (lasts 7 days or any duration)
            if (!string.IsNullOrEmpty(message.VehicleType))
            {
                cache.Set($"VehicleType_{message.VehicleId}", message.VehicleType, TimeSpan.FromDays(7));
            }
        };

        channel.BasicConsume("vehicle.status.changed", true, consumer);
        return Task.CompletedTask;
    }
}
