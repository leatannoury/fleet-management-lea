//trip consumes fleet events (local cache)
//trip updates its local cache when drivers are assigned
// trip service should not directly access fleet db
// trip service needs to know if a driver is available before assigning to a trip. 
// But it can't just query fleet db. so the solution is:
// when fleet assigns a driver, it publishes "driver.assigned" to RabbitMQ.
// Trip Service's DriverAssignedConsumer (running in background) receives that message
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

public class DriverAssignedConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public DriverAssignedConsumer(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        var connection = factory.CreateConnection();
        var channel = connection.CreateModel();

        channel.QueueDeclare("driver.assigned", false, false, false, null);

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += (model, ea) =>
        {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
            var message = JsonSerializer.Deserialize<DriverAssignedEvent>(json);

            using var scope = _serviceProvider.CreateScope();
            
            // Fetch the Memory Cache instead of Database
            var cache = scope.ServiceProvider.GetRequiredService<IMemoryCache>();

            // Save the driver's availability state directly into the server's RAM!
            cache.Set($"DriverStatus_{message.DriverId}", "Assigned");
            cache.Set($"VehicleStatus_{message.VehicleId}", "Assigned");
        };

        channel.BasicConsume("driver.assigned", true, consumer);

        return Task.CompletedTask;
    }
}
