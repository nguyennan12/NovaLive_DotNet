using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NovaLive.Application.Abstractions.ThirdParty;
using NovaLive.Contracts.V1.Payments;
using NovaLive.Infrastructure.Payments.Momo;
using Xunit;

namespace NovaLive.Infrastructure.Tests.Payments;

public class MomoPaymentAdapterTests
{
    private readonly MomoOptions _options = new()
    {
        PartnerCode = "MOMO_TEST",
        AccessKey = "ACCESS_KEY_123",
        SecretKey = "SECRET_KEY_456",
        Endpoint = "https://test-payment.momo.vn/v2/gateway/api/create",
        RedirectUrl = "https://novalive.vn/orders/payment-result",
        IpnUrl = "https://api.novalive.vn/api/v1/payments/webhook/momo",
        RequestType = "captureWallet"
    };

    [Fact]
    public void VerifySignature_WhenValidSignature_ReturnsTrue()
    {
        // Arrange
        var adapter = CreateAdapter();
        var rawHash = "accessKey=" + _options.AccessKey +
                      "&amount=50000" +
                      "&extraData=" +
                      "&message=Success" +
                      "&orderId=ORD-001" +
                      "&orderInfo=Order ORD-001" +
                      "&orderType=momo_wallet" +
                      "&partnerCode=" + _options.PartnerCode +
                      "&payType=qr" +
                      "&requestId=REQ-001" +
                      "&responseTime=1600000000" +
                      "&resultCode=0" +
                      "&transId=123456789";

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.SecretKey));
        var validSignature = Convert.ToHexStringLower(hmac.ComputeHash(Encoding.UTF8.GetBytes(rawHash)));

        var request = new MomoIpnRequest
        {
            PartnerCode = _options.PartnerCode,
            OrderId = "ORD-001",
            RequestId = "REQ-001",
            Amount = 50000,
            OrderInfo = "Order ORD-001",
            OrderType = "momo_wallet",
            TransId = 123456789,
            ResultCode = 0,
            Message = "Success",
            PayType = "qr",
            ResponseTime = 1600000000,
            ExtraData = "",
            Signature = validSignature
        };

        // Act
        var isValid = adapter.VerifySignature(request);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void VerifySignature_WhenTamperedAmount_ReturnsFalse()
    {
        // Arrange
        var adapter = CreateAdapter();
        var rawHash = "accessKey=" + _options.AccessKey +
                      "&amount=50000" +
                      "&extraData=" +
                      "&message=Success" +
                      "&orderId=ORD-001" +
                      "&orderInfo=Order ORD-001" +
                      "&orderType=momo_wallet" +
                      "&partnerCode=" + _options.PartnerCode +
                      "&payType=qr" +
                      "&requestId=REQ-001" +
                      "&responseTime=1600000000" +
                      "&resultCode=0" +
                      "&transId=123456789";

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.SecretKey));
        var signature = Convert.ToHexStringLower(hmac.ComputeHash(Encoding.UTF8.GetBytes(rawHash)));

        var request = new MomoIpnRequest
        {
            PartnerCode = _options.PartnerCode,
            OrderId = "ORD-001",
            RequestId = "REQ-001",
            Amount = 999999, // Tampered amount
            OrderInfo = "Order ORD-001",
            OrderType = "momo_wallet",
            TransId = 123456789,
            ResultCode = 0,
            Message = "Success",
            PayType = "qr",
            ResponseTime = 1600000000,
            ExtraData = "",
            Signature = signature
        };

        // Act
        var isValid = adapter.VerifySignature(request);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public async Task CreatePaymentUrlAsync_WhenSuccessResponse_ParsesCorrectly()
    {
        // Arrange
        var expectedResponse = new MomoCreatePaymentResponse
        {
            PartnerCode = _options.PartnerCode,
            OrderId = "ORD-123",
            RequestId = "REQ-123",
            Amount = 100000,
            ResponseTime = 1600000000,
            Message = "Success",
            ResultCode = 0,
            PayUrl = "https://pay.momo.vn/v2/gateway/pay?s=123456",
            QrCodeUrl = "https://pay.momo.vn/qr/123"
        };

        var handler = new FakeHttpMessageHandler(JsonSerializer.Serialize(expectedResponse));
        var client = new HttpClient(handler);
        var adapter = new MomoPaymentAdapter(client, Options.Create(_options), NullLogger<MomoPaymentAdapter>.Instance);

        // Act
        var response = await adapter.CreatePaymentUrlAsync("ORD-123", 100000, "Thanh toan don hang ORD-123");

        // Assert
        response.Should().NotBeNull();
        response.ResultCode.Should().Be(0);
        response.PayUrl.Should().Be("https://pay.momo.vn/v2/gateway/pay?s=123456");
        response.QrCodeUrl.Should().Be("https://pay.momo.vn/qr/123");
    }

    private MomoPaymentAdapter CreateAdapter(HttpClient? client = null)
    {
        var httpClient = client ?? new HttpClient(new FakeHttpMessageHandler("{}"));
        return new MomoPaymentAdapter(httpClient, Options.Create(_options), NullLogger<MomoPaymentAdapter>.Instance);
    }

    private sealed class FakeHttpMessageHandler(string responseContent) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        }
    }
}
