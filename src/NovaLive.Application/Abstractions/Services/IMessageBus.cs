namespace NovaLive.Application.Abstractions.Services;

public interface IMessageBus
{
    Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : class;
}
