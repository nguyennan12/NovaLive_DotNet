using NovaLive.Application.Common.Messaging;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Shipping.Commands.Webhook;

public sealed class HandleShippingWebhookCommandHandler(ShippingDeliveryProcessor delivery)
    : ICommandHandler<HandleShippingWebhookCommand>
{
    public Task<Result> Handle(HandleShippingWebhookCommand command, CancellationToken ct) =>
        delivery.ProcessAsync(command.Provider, command.TrackingCode, command.Status,
            command.RawPayload, command.IsVerifiedWebhook, ct);
}
