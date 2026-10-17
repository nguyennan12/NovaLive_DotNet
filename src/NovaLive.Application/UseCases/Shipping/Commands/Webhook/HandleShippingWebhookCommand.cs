using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using NovaLive.Application.Common.Messaging;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Shipping.Commands.Webhook;

public sealed record HandleShippingWebhookCommand(
    ShippingProvider Provider,
    string TrackingCode,
    ShippingStatus Status,
    string? RawPayload = null,
    DateTimeOffset? EventTimestamp = null) : ICommand, ITransactionalCommand, IIdempotentCommand
{
    public string IdempotencyKey => "shipping:webhook:" + Convert.ToHexString(SHA256.HashData(
        Encoding.UTF8.GetBytes($"{Provider}|{TrackingCode.Trim()}|{Status}|{EventTimestamp?.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture) ?? "none"}")));
}
