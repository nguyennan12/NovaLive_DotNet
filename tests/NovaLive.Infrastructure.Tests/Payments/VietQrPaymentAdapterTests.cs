using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NovaLive.Infrastructure.Payments.VietQr;
using Xunit;

namespace NovaLive.Infrastructure.Tests.Payments;

public sealed class VietQrPaymentAdapterTests
{
    private readonly VietQrOptions _options = new()
    {
        BankBin = "970422",
        BankCode = "MB",
        AccountNumber = "0000123456789",
        AccountName = "CONG TY NOVALIVE",
        ClientId = "sandbox-user",
        ApiKey = "sandbox-password",
        WebhookSecret = "legacy-secret",
        BaseUrl = "https://dev.vietqr.org",
        CallbackUsername = "callback-user",
        CallbackPassword = "callback-password",
        CallbackJwtSecret = "callback-jwt-secret-at-least-32-characters"
    };

    [Fact]
    public async Task GenerateDynamicQr_UsesDocumentedTokenThenGenerateContract()
    {
        var handler = new SequenceHandler(
            Json(HttpStatusCode.OK, """{"access_token":"provider-token","token_type":"Bearer","expires_in":300}"""),
            Json(HttpStatusCode.OK, """{"bankCode":"MB","bankAccount":"0000123456789","userBankName":"CONG TY NOVALIVE","amount":"150000","content":"NOVA ORD-001","qrCode":"000201010212payload","qrLink":"https://pro.vietqr.vn/qr-generated?token=ref"}"""));
        var adapter = Create(handler);

        var result = await adapter.GenerateDynamicQrAsync("ORD-001", 150000, "must not replace reconciliation content");

        result.QrCode.Should().Be("000201010212payload");
        result.QrImageBase64.Should().NotBeNullOrEmpty();
        handler.Requests.Should().HaveCount(2);
        handler.Requests[0].Path.Should().Be("/vqr/api/token_generate");
        handler.Requests[0].Authorization.Should().StartWith("Basic ");
        handler.Requests[1].Path.Should().Be("/vqr/api/qr/generate-customer");
        handler.Requests[1].Authorization.Should().Be("Bearer provider-token");
        handler.Requests[1].Body.Should().Contain("\"qrType\":0").And.Contain("\"transType\":\"C\"");
        using var requestBody = JsonDocument.Parse(handler.Requests[1].Body);
        requestBody.RootElement.GetProperty("content").GetString().Should().Be("NOVA ORD-001");
        var providerOrderId = requestBody.RootElement.GetProperty("orderId").GetString();
        providerOrderId.Should().HaveLength(13);
        adapter.IsExpectedOrderId("ORD-001", providerOrderId).Should().BeTrue();
        adapter.IsExpectedOrderId("ORD-OTHER", providerOrderId).Should().BeFalse();
    }

    [Fact]
    public async Task TriggerTestCallback_UsesSandboxEndpointAndSameQrValues()
    {
        var handler = new SequenceHandler(
            Json(HttpStatusCode.OK, """{"access_token":"provider-token","token_type":"Bearer","expires_in":300}"""),
            Json(HttpStatusCode.OK, """{"status":"SUCCESS","message":""}"""));
        var adapter = Create(handler);
        var qr = new NovaLive.Contracts.V1.Payments.VietQrResponse("payload", "link", "name", _options.AccountNumber, "MB", 150000, "NOVA ORD-001");

        var result = await adapter.TriggerTestCallbackAsync(qr);

        result.Status.Should().Be("SUCCESS");
        handler.Requests[1].Path.Should().Be("/vqr/bank/api/test/transaction-callback");
        handler.Requests[1].Body.Should().Contain("\"content\":\"NOVA ORD-001\"").And.Contain("\"amount\":150000");
    }

    [Fact]
    public void CallbackToken_RoundTripsAndRejectsWrongCredentials()
    {
        var adapter = Create(new SequenceHandler());
        adapter.TryIssueCallbackToken("callback-user", "wrong", out _).Should().BeFalse();
        adapter.TryIssueCallbackToken("callback-user", "callback-password", out var response).Should().BeTrue();
        response.Should().NotBeNull();
        adapter.VerifyCallbackToken(response!.AccessToken).Should().BeTrue();
    }

    [Theory]
    [InlineData("NOVA ORD-12345", "ORD-12345")]
    [InlineData("VQR ORD-456", "ORD-456")]
    [InlineData("Chuyen tien NOVA ORD-777", "ORD-777")]
    public void ExtractOrderCode_ReturnsExpectedValue(string content, string expected) =>
        Create(new SequenceHandler()).ExtractOrderCode(content).Should().Be(expected);

    [Fact]
    public void LegacyWebhookSignature_IsStillValidatedDuringMigration()
    {
        var adapter = Create(new SequenceHandler());
        const string payload = "{\"amount\":150000}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.WebhookSecret));
        var signature = Convert.ToHexStringLower(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)));
        adapter.VerifyWebhookSignature(payload, signature).Should().BeTrue();
        adapter.VerifyWebhookSignature(payload, "bad").Should().BeFalse();
    }

    private VietQrPaymentAdapter Create(HttpMessageHandler handler) => new(
        new HttpClient(handler), Options.Create(_options), NullLogger<VietQrPaymentAdapter>.Instance);

    private static HttpResponseMessage Json(HttpStatusCode status, string body) => new(status)
    {
        Content = new StringContent(body, Encoding.UTF8, "application/json")
    };

    private sealed class SequenceHandler(params HttpResponseMessage[] responses) : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> _responses = new(responses);
        public List<CapturedRequest> Requests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(new CapturedRequest(request.RequestUri!.AbsolutePath,
                request.Headers.Authorization?.ToString() ?? string.Empty,
                request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken)));
            return _responses.Dequeue();
        }
    }

    private sealed record CapturedRequest(string Path, string Authorization, string Body);
}
