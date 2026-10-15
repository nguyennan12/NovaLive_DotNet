using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NovaLive.Api.Controllers.Payments;
using NovaLive.Application.Abstractions.ThirdParty;
using NovaLive.Application.UseCases.Payments;
using NovaLive.Application.UseCases.Payments.Commands.HandleMomoIpn;
using NovaLive.Application.UseCases.Payments.Commands.HandleVietQrWebhook;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Payments;
using NovaLive.Domain.Common;
using Xunit;

namespace NovaLive.Api.Tests.Payments;

public class PaymentsControllerTests
{
    private readonly Mock<ISender> _senderMock = new();
    private readonly Mock<IMomoPaymentAdapter> _momoAdapterMock = new();
    private readonly Mock<IVietQrPaymentAdapter> _vietQrAdapterMock = new();
    private readonly PaymentsController _controller;

    public PaymentsControllerTests()
    {
        _controller = new PaymentsController(
            _senderMock.Object,
            _momoAdapterMock.Object,
            _vietQrAdapterMock.Object,
            NullLogger<PaymentsController>.Instance)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Fact]
    public async Task MomoIpn_WhenSuccess_ReturnsOk()
    {
        // Arrange
        var request = new MomoIpnRequest
        {
            PartnerCode = "MOMO",
            OrderId = "ORD-001",
            TransId = 123456,
            ResultCode = 0,
            Amount = 50000
        };

        _senderMock.Setup(s => s.Send(It.IsAny<HandleMomoIpnCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new MomoIpnResponse(0, "OK")));

        // Act
        var result = await _controller.MomoIpn(request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<MomoIpnResponse>().Subject;
        response.ResultCode.Should().Be(0);
        response.Message.Should().Be("OK");
    }

    [Fact]
    public async Task MomoIpn_WhenFailure_ReturnsBadRequestWithResultCode99()
    {
        // Arrange
        var request = new MomoIpnRequest
        {
            PartnerCode = "MOMO",
            OrderId = "ORD-001",
            TransId = 123456,
            ResultCode = 0,
            Amount = 50000
        };

        _senderMock.Setup(s => s.Send(It.IsAny<HandleMomoIpnCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<MomoIpnResponse>(PaymentErrors.InvalidSignature));

        // Act
        var result = await _controller.MomoIpn(request, CancellationToken.None);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = badRequestResult.Value.Should().BeOfType<MomoIpnResponse>().Subject;
        response.ResultCode.Should().Be(99);
    }

    [Fact]
    public async Task VietQrWebhook_WhenSuccess_ReturnsOk()
    {
        // Arrange
        var request = new VietQrWebhookRequest
        {
            TransactionId = "TXN_001",
            Amount = 100000,
            Content = "NOVA ORD-123"
        };

        _senderMock.Setup(s => s.Send(It.IsAny<HandleVietQrWebhookCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new VietQrWebhookResponse(true, "OK", "ORD-123")));

        // Act
        var result = await _controller.VietQrWebhook(request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<VietQrWebhookResponse>().Subject;
        response.Success.Should().BeTrue();
        response.OrderCode.Should().Be("ORD-123");
    }

    [Fact]
    public async Task VietQrWebhook_WhenFailure_ReturnsBadRequest()
    {
        // Arrange
        var request = new VietQrWebhookRequest
        {
            TransactionId = "TXN_002",
            Amount = 100000,
            Content = "Invalid content"
        };

        _senderMock.Setup(s => s.Send(It.IsAny<HandleVietQrWebhookCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<VietQrWebhookResponse>(PaymentErrors.CannotParseOrderCode));

        // Act
        var result = await _controller.VietQrWebhook(request, CancellationToken.None);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = badRequestResult.Value.Should().BeOfType<VietQrWebhookResponse>().Subject;
        response.Success.Should().BeFalse();
    }

    [Fact]
    public void VietQrPing_ReturnsOkWithActiveStatus()
    {
        // Act
        var result = _controller.VietQrPing();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateMomoPayment_CallsAdapterAndReturnsOk()
    {
        // Arrange
        var request = new CreateMomoPaymentUrlRequest(
            OrderId: "ORD-001",
            Amount: 100000,
            OrderInfo: "Thanh toan ORD-001");

        var momoResponse = new MomoCreatePaymentResponse
        {
            PartnerCode = "MOMO",
            OrderId = "ORD-001",
            Amount = 100000,
            ResultCode = 0,
            PayUrl = "https://pay.momo.vn/pay",
            QrCodeUrl = "https://pay.momo.vn/qr"
        };

        _momoAdapterMock.Setup(m => m.CreatePaymentUrlAsync("ORD-001", 100000, "Thanh toan ORD-001", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(momoResponse);

        // Act
        var result = await _controller.CreateMomoPayment(request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var apiResponse = okResult.Value.Should().BeAssignableTo<ApiResponse<MomoCreatePaymentResponse>>().Subject;
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.PayUrl.Should().Be("https://pay.momo.vn/pay");
    }

    [Fact]
    public void GenerateVietQr_CallsAdapterAndReturnsOk()
    {
        // Arrange
        var qrResponse = new VietQrResponse(
            QrCode: "000201...",
            QrDataUrl: "https://img.vietqr.io/...",
            AccountName: "CONG TY NOVALIVE",
            AccountNumber: "0000123456789",
            BankBin: "970422",
            Amount: 150000,
            Description: "NOVA ORD-001");

        _vietQrAdapterMock.Setup(v => v.GenerateDynamicQr("ORD-001", 150000, null))
            .Returns(qrResponse);

        // Act
        var result = _controller.GenerateVietQr("ORD-001", 150000, null);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var apiResponse = okResult.Value.Should().BeAssignableTo<ApiResponse<VietQrResponse>>().Subject;
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Description.Should().Be("NOVA ORD-001");
    }
}
