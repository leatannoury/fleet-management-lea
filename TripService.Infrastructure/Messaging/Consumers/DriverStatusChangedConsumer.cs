//trip consumes fleet events (local cache)
//trip updates its local cache when drivers are assigned
// trip service should not directly access fleet db
// trip service needs to know the driver status before assigning to a trip. 
// But it can't just query fleet db. so the solution is:
// when fleet changes driver status, it publishes "driver.status.changed" to RabbitMQ.
// Trip Service's Driver status changed consumer (running in background) receives that message
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

namespace TripService.Infrastructure.Messaging.Consumers;

public class DriverStatusChangedConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public DriverStatusChangedConsumer(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        var connection = factory.CreateConnection();
        var channel = connection.CreateModel();

        channel.QueueDeclare("driver.status.changed", false, false, false, null);

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += (model, ea) =>
        {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
            var message = JsonSerializer.Deserialize<DriverStatusChangedEvent>(json);

            using var scope = _serviceProvider.CreateScope();
            var cache = scope.ServiceProvider.GetRequiredService<IMemoryCache>();

            cache.Set($"DriverStatus_{message.DriverId}", message.Status);
        };

        channel.BasicConsume("driver.status.changed", true, consumer);

        return Task.CompletedTask;
    }
}
