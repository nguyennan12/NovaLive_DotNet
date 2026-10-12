using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Shipping;

namespace NovaLive.Application.UseCases.Shipping.Commands.Webhook;

public record HandleShippingWebhookCommand(GhnWebhookRequest Request, string RawPayload)
    : ICommand, ITransactionalCommand;
