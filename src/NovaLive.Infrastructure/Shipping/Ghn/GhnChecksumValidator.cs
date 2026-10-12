using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace NovaLive.Infrastructure.Shipping.Ghn;

public sealed class GhnChecksumValidator(IOptions<GhnOptions> options) : IGhnChecksumValidator
{
    private readonly GhnOptions _options = options.Value;

    public bool ValidateWebhook(string rawPayload, string? secretHeader, string? signatureHeader = null, string? tokenHeader = null)
    {
        // 1. Kiểm tra secretHeader (như cấu hình Header X-Webhook-Secret tùy chỉnh trong GHN Portal)
        if (!string.IsNullOrWhiteSpace(secretHeader) && ValidateSecret(secretHeader))
        {
            return true;
        }

        // 2. Kiểm tra signatureHeader (X-Ghn-Signature)
        if (!string.IsNullOrWhiteSpace(signatureHeader) && ValidateSignature(rawPayload, signatureHeader))
        {
            return true;
        }

        // 3. Kiểm tra tokenHeader (Token)
        if (!string.IsNullOrWhiteSpace(tokenHeader) && ValidateToken(tokenHeader))
        {
            return true;
        }

        return false;
    }

    public bool ValidateSecret(string? secret)
    {
        if (string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(_options.WebhookSecret))
        {
            return false;
        }

        return FixedTimeEquals(secret, _options.WebhookSecret);
    }

    public bool ValidateSignature(string payload, string? signature)
    {
        if (string.IsNullOrWhiteSpace(signature) || string.IsNullOrWhiteSpace(_options.WebhookSecret))
        {
            return false;
        }

        if (FixedTimeEquals(signature, _options.WebhookSecret))
        {
            return true;
        }

        try
        {
            var keyBytes = Encoding.UTF8.GetBytes(_options.WebhookSecret);
            var payloadBytes = Encoding.UTF8.GetBytes(payload);

            using var hmac = new HMACSHA256(keyBytes);
            var computedHash = hmac.ComputeHash(payloadBytes);
            var computedHex = Convert.ToHexStringLower(computedHash);

            return FixedTimeEquals(signature, computedHex);
        }
        catch
        {
            return false;
        }
    }

    private bool ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        if (FixedTimeEquals(token, _options.WebhookSecret))
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(_options.ApiToken) && FixedTimeEquals(token, _options.ApiToken))
        {
            return true;
        }

        return false;
    }

    private static bool FixedTimeEquals(string left, string right)
    {
        var leftBytes = Encoding.UTF8.GetBytes(left);
        var rightBytes = Encoding.UTF8.GetBytes(right);

        return CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
    }
}
