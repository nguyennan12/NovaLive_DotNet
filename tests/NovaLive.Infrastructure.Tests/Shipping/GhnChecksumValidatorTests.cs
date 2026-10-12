using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using NovaLive.Infrastructure.Shipping.Ghn;
using Xunit;

namespace NovaLive.Infrastructure.Tests.Shipping;

public class GhnChecksumValidatorTests
{
    private readonly GhnOptions _options = new()
    {
        WebhookSecret = "ff432d05-3989-11f1-a973-aee5264794df",
        ApiToken = "ff432d05-3989-11f1-a973-aee5264794df"
    };

    [Fact]
    public void ValidateWebhook_WithMatchingSecretHeader_ReturnsTrue()
    {
        var validator = new GhnChecksumValidator(Options.Create(_options));
        var result = validator.ValidateWebhook("{\"test\":1}", secretHeader: "ff432d05-3989-11f1-a973-aee5264794df");

        Assert.True(result);
    }

    [Fact]
    public void ValidateWebhook_WithInvalidSecretHeader_ReturnsFalse()
    {
        var validator = new GhnChecksumValidator(Options.Create(_options));
        var result = validator.ValidateWebhook("{\"test\":1}", secretHeader: "wrong_secret_key");

        Assert.False(result);
    }

    [Fact]
    public void ValidateWebhook_WithValidHmacSignatureHeader_ReturnsTrue()
    {
        var validator = new GhnChecksumValidator(Options.Create(_options));
        var payload = "{\"OrderCode\":\"GHN123\",\"Status\":\"delivering\"}";

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.WebhookSecret));
        var expectedHash = Convert.ToHexStringLower(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)));

        var result = validator.ValidateWebhook(payload, secretHeader: null, signatureHeader: expectedHash);

        Assert.True(result);
    }

    [Fact]
    public void ValidateWebhook_WithTamperedPayload_ReturnsFalse()
    {
        var validator = new GhnChecksumValidator(Options.Create(_options));
        var payload = "{\"OrderCode\":\"GHN123\",\"Status\":\"delivering\"}";

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.WebhookSecret));
        var expectedHash = Convert.ToHexStringLower(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)));

        var tamperedPayload = "{\"OrderCode\":\"GHN123\",\"Status\":\"delivered\"}";
        var result = validator.ValidateWebhook(tamperedPayload, secretHeader: null, signatureHeader: expectedHash);

        Assert.False(result);
    }

    [Fact]
    public void ValidateWebhook_WithMatchingTokenHeader_ReturnsTrue()
    {
        var validator = new GhnChecksumValidator(Options.Create(_options));
        var result = validator.ValidateWebhook("{}", secretHeader: null, signatureHeader: null, tokenHeader: "ff432d05-3989-11f1-a973-aee5264794df");

        Assert.True(result);
    }
}
