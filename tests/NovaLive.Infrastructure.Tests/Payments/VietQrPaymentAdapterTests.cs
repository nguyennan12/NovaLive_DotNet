using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NovaLive.Infrastructure.Payments.VietQr;
using Xunit;

namespace NovaLive.Infrastructure.Tests.Payments;

public class VietQrPaymentAdapterTests
{
    private readonly VietQrOptions _options = new()
    {
        BankBin = "970422",
        AccountNumber = "0000123456789",
        AccountName = "CONG TY NOVALIVE",
        ClientId = "customer-nan-user26703",
        ApiKey = "API_KEY_TEST",
        WebhookSecret = "SECRET_KEY_TEST",
        BaseUrl = "https://api.vietqr.vn"
    };

    [Fact]
    public void GenerateDynamicQr_GeneratesValidQrAndQuickLink()
    {
        // Arrange
        var adapter = new VietQrPaymentAdapter(Options.Create(_options), NullLogger<VietQrPaymentAdapter>.Instance);

        // Act
        var result = adapter.GenerateDynamicQr("ORD-20261015-001", 150000);

        // Assert
        result.Should().NotBeNull();
        result.Amount.Should().Be(150000);
        result.Description.Should().Be("NOVA ORD-20261015-001");
        result.QrCode.Should().StartWith("000201010212"); // EMVCo format
        result.QrCode.Should().Contain("970422"); // MBBank BIN
        result.QrCode.Should().Contain("0000123456789");
        result.QrDataUrl.Should().Contain("img.vietqr.io/image/970422-0000123456789-compact2.png");
        result.QrDataUrl.Should().Contain("amount=150000");
    }

    [Theory]
    [InlineData("NOVA ORD-12345", "ORD-12345")]
    [InlineData("nova ord-12345", "ord-12345")]
    [InlineData("NOVA_ORD-999", "ORD-999")]
    [InlineData("NOVA-ORD-888", "ORD-888")]
    [InlineData("VQR ORD-456", "ORD-456")]
    [InlineData("Chuyen tien mua hang NOVA ORD-777 tu app MB", "ORD-777")]
    [InlineData("NOVA12345", "12345")]
    public void ExtractOrderCode_WhenSyntaxPresent_ReturnsExtractedCode(string content, string expected)
    {
        // Arrange
        var adapter = new VietQrPaymentAdapter(Options.Create(_options), NullLogger<VietQrPaymentAdapter>.Instance);

        // Act
        var orderCode = adapter.ExtractOrderCode(content);

        // Assert
        orderCode.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Chuyen tien an trua")]
    [InlineData("Tien thue nha thang 10")]
    public void ExtractOrderCode_WhenSyntaxNotPresent_ReturnsNull(string content)
    {
        // Arrange
        var adapter = new VietQrPaymentAdapter(Options.Create(_options), NullLogger<VietQrPaymentAdapter>.Instance);

        // Act
        var orderCode = adapter.ExtractOrderCode(content);

        // Assert
        orderCode.Should().BeNull();
    }

    [Fact]
    public void VerifyWebhookSignature_WhenSignatureMatches_ReturnsTrue()
    {
        // Arrange
        var adapter = new VietQrPaymentAdapter(Options.Create(_options), NullLogger<VietQrPaymentAdapter>.Instance);
        var rawPayload = "{\"id\":123,\"amount\":150000,\"content\":\"NOVA ORD-001\"}";

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.WebhookSecret));
        var validSignature = Convert.ToHexStringLower(hmac.ComputeHash(Encoding.UTF8.GetBytes(rawPayload)));

        // Act
        var isValid = adapter.VerifyWebhookSignature(rawPayload, validSignature);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void VerifyWebhookSignature_WhenTampered_ReturnsFalse()
    {
        // Arrange
        var adapter = new VietQrPaymentAdapter(Options.Create(_options), NullLogger<VietQrPaymentAdapter>.Instance);
        var rawPayload = "{\"id\":123,\"amount\":150000,\"content\":\"NOVA ORD-001\"}";

        // Act
        var isValid = adapter.VerifyWebhookSignature(rawPayload, "invalid_signature");

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void VerifyWebhookSignature_WhenWebhookSecretNotConfigured_ReturnsFalse()
    {
        // Arrange
        var unconfiguredOptions = new VietQrOptions { WebhookSecret = "" };
        var adapter = new VietQrPaymentAdapter(Options.Create(unconfiguredOptions), NullLogger<VietQrPaymentAdapter>.Instance);

        // Act
        var isValid = adapter.VerifyWebhookSignature("{}", "any_sig");

        // Assert
        isValid.Should().BeFalse();
    }
}


