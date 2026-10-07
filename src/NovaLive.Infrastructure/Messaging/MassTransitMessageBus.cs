using MassTransit;
using NovaLive.Application.Abstractions.Messaging;

namespace NovaLive.Infrastructure.Messaging;

public sealed class MassTransitMessageBus(IPublishEndpoint publishEndpoint) : IMessageBus
{
    public Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : class
    {
        return publishEndpoint.Publish(message, cancellationToken);
    }
}
