// class that send messages (events) to rabbitMQ 
//event publisher
//actual rabbitmq implementation
//both trip and fleet can use it
//when the app starts it connects to rabbitmq and keep the conversation open.

namespace SharedKernel.Messaging;

using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

public class RabbitMqPublisher : IEventPublisher
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMqPublisher()
    {
        var factory = new ConnectionFactory()
        {
            HostName = "localhost" // rabbit mq server
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    //when .publish is called: ensures queue exists, if not it creates it
    //then it serializes the C# object to json then bytes
    //then it sends those bytes to rabbitmq to the named queue
    public void Publish<T>(string queueName, T message)
    {
        // queue declare it ensures that the queue exists if not ot creates it
        _channel.QueueDeclare(
            queue: queueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        // serialize: it convert the message -> json -> bytes 
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        //basic publish: it sends the message to the queue
        _channel.BasicPublish(
            exchange: "",
            routingKey: queueName,
            basicProperties: null,
            body: body);
    }
}