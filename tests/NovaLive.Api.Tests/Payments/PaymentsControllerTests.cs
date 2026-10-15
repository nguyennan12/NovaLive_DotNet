using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NovaLive.Api.Controllers.Payments;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.ThirdParty;
using NovaLive.Application.UseCases.Payments;
using NovaLive.Application.UseCases.Payments.Commands.HandleMomoIpn;
using NovaLive.Application.UseCases.Payments.Commands.HandleVietQrWebhook;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Payments;
using NovaLive.Domain.Common;
using NovaLive.Domain.Orders;
using NovaLive.Domain.Payments;
using Xunit;

namespace NovaLive.Api.Tests.Payments;

public class PaymentsControllerTests
{
    private readonly Mock<ISender> _senderMock = new();
    private readonly Mock<IMomoPaymentAdapter> _momoAdapterMock = new();
    private readonly Mock<IVietQrPaymentAdapter> _vietQrAdapterMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly Mock<IParentOrderRepository> _parentOrderRepositoryMock = new();
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock = new();
    private readonly PaymentsController _controller;
    private readonly Guid _currentUserId = Guid.NewGuid();

    public PaymentsControllerTests()
    {
        _currentUserMock.SetupGet(c => c.UserId).Returns(_currentUserId);

        _controller = new PaymentsController(
            _senderMock.Object,
            _momoAdapterMock.Object,
            _vietQrAdapterMock.Object,
            _currentUserMock.Object,
            _parentOrderRepositoryMock.Object,
            _paymentRepositoryMock.Object,
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
    public async Task CreateMomoPayment_WhenOrderValid_LoadsServerAmountAndReturnsOk()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var parentOrder = ParentOrder.Create(_currentUserId, "ORD-001", "{}", 100000m, 0m, 0m, 100000m, null, now);
        var payment = Payment.Create(parentOrder.Id, PaymentMethod.MoMo, 100000m, now);

        _parentOrderRepositoryMock.Setup(p => p.FindByOrderCodeAsync("ORD-001", _currentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(parentOrder);
        _paymentRepositoryMock.Setup(p => p.GetByParentOrderIdAsync(parentOrder.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        var request = new CreateMomoPaymentUrlRequest(
            OrderId: "ORD-001",
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
    public async Task CreateMomoPayment_WhenOrderNotFound_ReturnsNotFound()
    {
        // Arrange
        _parentOrderRepositoryMock.Setup(p => p.FindByOrderCodeAsync("NON-EXISTENT", _currentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ParentOrder?)null);

        var request = new CreateMomoPaymentUrlRequest(OrderId: "NON-EXISTENT");

        // Act
        var result = await _controller.CreateMomoPayment(request, CancellationToken.None);

        // Assert
        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task GenerateVietQr_WhenOrderValid_LoadsServerAmountAndReturnsOk()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var parentOrder = ParentOrder.Create(_currentUserId, "ORD-001", "{}", 150000m, 0m, 0m, 150000m, null, now);
        var payment = Payment.Create(parentOrder.Id, PaymentMethod.VietQR, 150000m, now);

        _parentOrderRepositoryMock.Setup(p => p.FindByOrderCodeAsync("ORD-001", _currentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(parentOrder);
        _paymentRepositoryMock.Setup(p => p.GetByParentOrderIdAsync(parentOrder.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

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
        var result = await _controller.GenerateVietQr("ORD-001", null, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var apiResponse = okResult.Value.Should().BeAssignableTo<ApiResponse<VietQrResponse>>().Subject;
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Description.Should().Be("NOVA ORD-001");
    }

    [Fact]
    public async Task GenerateVietQr_WhenOrderNotFound_ReturnsNotFound()
    {
        // Arrange
        _parentOrderRepositoryMock.Setup(p => p.FindByOrderCodeAsync("NON-EXISTENT", _currentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ParentOrder?)null);

        // Act
        var result = await _controller.GenerateVietQr("NON-EXISTENT", null, CancellationToken.None);

        // Assert
        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }
}
