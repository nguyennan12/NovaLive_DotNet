using MassTransit;
using NovaLive.Application.Abstractions.Services;

namespace NovaLive.Infrastructure.Services;

public sealed class MassTransitMessageBus(IPublishEndpoint publishEndpoint) : IMessageBus
{
    public Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : class
    {
        return publishEndpoint.Publish(message, cancellationToken);
    }
}
