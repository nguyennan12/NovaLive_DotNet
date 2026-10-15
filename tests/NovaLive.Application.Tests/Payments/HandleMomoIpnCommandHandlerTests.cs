using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Abstractions.ThirdParty;
using NovaLive.Application.UseCases.Payments;
using NovaLive.Application.UseCases.Payments.Commands.HandleMomoIpn;
using NovaLive.Contracts.V1.Payments;
using NovaLive.Domain.Common;
using NovaLive.Domain.Orders;
using NovaLive.Domain.Payments;
using NovaLive.Domain.Pricing;
using NovaLive.Domain.Shops;
using NovaLive.Domain.System;
using Xunit;

namespace NovaLive.Application.Tests.Payments;

public class HandleMomoIpnCommandHandlerTests
{
    private readonly Mock<IMomoPaymentAdapter> _momoAdapterMock = new();
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock = new();
    private readonly Mock<IParentOrderRepository> _parentOrderRepositoryMock = new();
    private readonly Mock<ISubOrderRepository> _subOrderRepositoryMock = new();
    private readonly Mock<IShopWalletRepository> _shopWalletRepositoryMock = new();
    private readonly Mock<IOutboxRepository> _outboxRepositoryMock = new();
    private readonly Mock<IDateTimeProvider> _clockMock = new();
    private readonly DateTimeOffset _now = new(2026, 10, 15, 10, 0, 0, TimeSpan.Zero);

    private readonly HandleMomoIpnCommandHandler _handler;

    public HandleMomoIpnCommandHandlerTests()
    {
        _clockMock.SetupGet(c => c.UtcNow).Returns(_now);

        _handler = new HandleMomoIpnCommandHandler(
            _momoAdapterMock.Object,
            _paymentRepositoryMock.Object,
            _parentOrderRepositoryMock.Object,
            _subOrderRepositoryMock.Object,
            _shopWalletRepositoryMock.Object,
            _outboxRepositoryMock.Object,
            _clockMock.Object,
            NullLogger<HandleMomoIpnCommandHandler>.Instance);
    }

    [Fact]
    public async Task Handle_WhenValidSuccessIpn_UpdatesPaymentParentOrderSubOrderCreditsWalletAndOutbox()
    {
        // Arrange
        var shopId = Guid.NewGuid();
        var orderCode = "ORD-MOMO-001";
        var amount = 200000L;
        var transId = 987654321L;

        var request = new MomoIpnRequest
        {
            PartnerCode = "MOMO",
            OrderId = orderCode,
            RequestId = Guid.NewGuid().ToString(),
            Amount = amount,
            OrderInfo = "Order ORD-MOMO-001",
            OrderType = "momo_wallet",
            TransId = transId,
            ResultCode = 0,
            Message = "Thành công.",
            PayType = "qr",
            ResponseTime = _now.ToUnixTimeMilliseconds(),
            Signature = "valid_signature"
        };

        _momoAdapterMock.Setup(m => m.VerifySignature(request)).Returns(true);
        _paymentRepositoryMock.Setup(p => p.GetByTransactionRefAsync(transId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        var parentOrder = CreateTestParentOrder(orderCode, 200000m, _now.AddMinutes(-5));
        var payment = Payment.Create(parentOrder.Id, PaymentMethod.MoMo, 200000m, _now.AddMinutes(-5));
        var subOrder = CreateTestSubOrder(parentOrder.Id, $"{orderCode}-1", shopId, 200000m, _now.AddMinutes(-5));

        _parentOrderRepositoryMock.Setup(p => p.GetByOrderCodeForUpdateAsync(orderCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(parentOrder);
        _paymentRepositoryMock.Setup(p => p.GetForUpdateAsync(parentOrder.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);
        _subOrderRepositoryMock.Setup(s => s.GetByParentIdsAsync(It.Is<IEnumerable<Guid>>(ids => ids.Contains(parentOrder.Id)), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync([subOrder]);

        var wallet = new ShopWallet(shopId);
        _shopWalletRepositoryMock.Setup(w => w.GetByShopIdAsync(shopId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);

        // Act
        var result = await _handler.Handle(new HandleMomoIpnCommand(request), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ResultCode.Should().Be(0);

        payment.Status.Should().Be(PaymentStatus.Success);
        payment.PaidAt.Should().Be(_now);
        payment.TransactionRef.Should().Be(transId.ToString());

        parentOrder.PaymentStatus.Should().Be(ParentOrderPaymentStatus.Paid);
        subOrder.Status.Should().Be(SubOrderStatus.Confirmed);

        wallet.Balance.Should().Be(200000m);
        _shopWalletRepositoryMock.Verify(w => w.AddTransactionAsync(
            It.Is<ShopWalletTransaction>(t => t.Type == WalletTxType.OrderRevenue && t.Amount == 200000m),
            It.IsAny<CancellationToken>()), Times.Once);

        _outboxRepositoryMock.Verify(o => o.AddAsync(
            It.Is<OutboxMessage>(m => m.EventType == "PaymentSuccessEvent"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenInvalidSignature_ReturnsInvalidSignatureError()
    {
        // Arrange
        var request = new MomoIpnRequest
        {
            PartnerCode = "MOMO",
            OrderId = "ORD-999",
            RequestId = "REQ-999",
            Amount = 100000,
            TransId = 12345,
            ResultCode = 0,
            Signature = "tampered_signature"
        };

        _momoAdapterMock.Setup(m => m.VerifySignature(request)).Returns(false);

        // Act
        var result = await _handler.Handle(new HandleMomoIpnCommand(request), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PaymentErrors.InvalidSignature);
        _paymentRepositoryMock.Verify(p => p.GetByTransactionRefAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenAlreadyPaid_IsIdempotentAndReturnsSuccess()
    {
        // Arrange
        var transId = 11223344L;
        var existingPayment = Payment.Create(Guid.NewGuid(), PaymentMethod.MoMo, 50000m, _now.AddMinutes(-10));
        existingPayment.MarkSuccess(transId.ToString(), _now.AddMinutes(-5));

        var request = new MomoIpnRequest
        {
            PartnerCode = "MOMO",
            OrderId = "ORD-ALREADY-PAID",
            TransId = transId,
            ResultCode = 0,
            Amount = 50000,
            Signature = "valid_sig"
        };

        _momoAdapterMock.Setup(m => m.VerifySignature(request)).Returns(true);
        _paymentRepositoryMock.Setup(p => p.GetByTransactionRefAsync(transId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPayment);

        // Act
        var result = await _handler.Handle(new HandleMomoIpnCommand(request), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ResultCode.Should().Be(0);
        result.Value.Message.Should().Contain("already processed");

        _shopWalletRepositoryMock.Verify(w => w.AddTransactionAsync(It.IsAny<ShopWalletTransaction>(), It.IsAny<CancellationToken>()), Times.Never);
        _outboxRepositoryMock.Verify(o => o.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenPaymentNotFound_ReturnsPaymentNotFoundError()
    {
        // Arrange
        var request = new MomoIpnRequest
        {
            PartnerCode = "MOMO",
            OrderId = "NON_EXISTENT",
            TransId = 99999,
            ResultCode = 0,
            Amount = 50000,
            Signature = "valid_sig"
        };

        _momoAdapterMock.Setup(m => m.VerifySignature(request)).Returns(true);
        _paymentRepositoryMock.Setup(p => p.GetByTransactionRefAsync("99999", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);
        _parentOrderRepositoryMock.Setup(p => p.GetByOrderCodeForUpdateAsync("NON_EXISTENT", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ParentOrder?)null);

        // Act
        var result = await _handler.Handle(new HandleMomoIpnCommand(request), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PaymentErrors.PaymentNotFound);
    }

    [Fact]
    public async Task Handle_WhenResultCodeFailed_MarksPaymentFailedAndCancelsParentOrder()
    {
        // Arrange
        var orderCode = "ORD-FAIL-001";
        var transId = 55667788L;
        var request = new MomoIpnRequest
        {
            PartnerCode = "MOMO",
            OrderId = orderCode,
            TransId = transId,
            ResultCode = 1006, // Người dùng từ chối thanh toán
            Message = "Giao dịch bị từ chối bởi người dùng.",
            Amount = 50000,
            Signature = "valid_sig"
        };

        _momoAdapterMock.Setup(m => m.VerifySignature(request)).Returns(true);
        _paymentRepositoryMock.Setup(p => p.GetByTransactionRefAsync(transId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        var parentOrder = CreateTestParentOrder(orderCode, 50000m, _now.AddMinutes(-5));
        var payment = Payment.Create(parentOrder.Id, PaymentMethod.MoMo, 50000m, _now.AddMinutes(-5));

        _parentOrderRepositoryMock.Setup(p => p.GetByOrderCodeForUpdateAsync(orderCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(parentOrder);
        _paymentRepositoryMock.Setup(p => p.GetForUpdateAsync(parentOrder.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        // Act
        var result = await _handler.Handle(new HandleMomoIpnCommand(request), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        payment.Status.Should().Be(PaymentStatus.Failed);
        parentOrder.PaymentStatus.Should().Be(ParentOrderPaymentStatus.Cancelled);
        _shopWalletRepositoryMock.Verify(w => w.AddTransactionAsync(It.IsAny<ShopWalletTransaction>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static ParentOrder CreateTestParentOrder(string orderCode, decimal total, DateTimeOffset now)
    {
        return ParentOrder.Create(Guid.NewGuid(), orderCode, "{}", total, 0m, 0m, total, null, now);
    }

    private static SubOrder CreateTestSubOrder(Guid parentId, string subOrderCode, Guid shopId, decimal total, DateTimeOffset now)
    {
        var price = new ShopCalculationResult(shopId, total, 0m, 0m, 0m, 0m, total, total, []);
        return SubOrder.Create(parentId, subOrderCode, Guid.NewGuid(), price, false, now);
    }
}
