using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Payments;

namespace NovaLive.Application.UseCases.Payments.Commands.HandleVietQrWebhook;

public sealed record HandleVietQrWebhookCommand(
    VietQrWebhookRequest Request,
    string RawBody,
    string? Signature = null,
    bool IsAuthenticatedCallback = false)
    : ICommand<VietQrWebhookResponse>, ITransactionalCommand;
