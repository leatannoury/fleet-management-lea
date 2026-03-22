//interface defining how to publish

namespace SharedKernel.Messaging;

public interface IEventPublisher
{
    void Publish<T>(string queueName, T message);
}