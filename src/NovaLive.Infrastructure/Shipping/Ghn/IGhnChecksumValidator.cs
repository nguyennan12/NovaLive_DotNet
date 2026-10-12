namespace NovaLive.Infrastructure.Shipping.Ghn;

public interface IGhnChecksumValidator
{
    bool ValidateWebhook(string rawPayload, string? secretHeader, string? signatureHeader = null, string? tokenHeader = null);
    bool ValidateSignature(string payload, string? signature);
    bool ValidateSecret(string? secret);
}
