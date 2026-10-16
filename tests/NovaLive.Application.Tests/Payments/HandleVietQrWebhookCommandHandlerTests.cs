using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Abstractions.ThirdParty;
using NovaLive.Application.UseCases.Payments;
using NovaLive.Application.UseCases.Payments.Commands.HandleVietQrWebhook;
using NovaLive.Contracts.V1.Payments;
using NovaLive.Domain.Common;
using NovaLive.Domain.Orders;
using NovaLive.Domain.Payments;
using NovaLive.Domain.Pricing;
using NovaLive.Domain.Shops;
using NovaLive.Domain.System;
using Xunit;

namespace NovaLive.Application.Tests.Payments;

public class HandleVietQrWebhookCommandHandlerTests
{
    private readonly Mock<IVietQrPaymentAdapter> _vietQrAdapterMock = new();
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock = new();
    private readonly Mock<IParentOrderRepository> _parentOrderRepositoryMock = new();
    private readonly Mock<ISubOrderRepository> _subOrderRepositoryMock = new();
    private readonly Mock<IShopWalletRepository> _shopWalletRepositoryMock = new();
    private readonly Mock<IOutboxRepository> _outboxRepositoryMock = new();
    private readonly Mock<IFlashSaleStockService> _flashSales = new();
    private readonly Mock<IDateTimeProvider> _clockMock = new();
    private readonly DateTimeOffset _now = new(2026, 10, 15, 10, 0, 0, TimeSpan.Zero);

    private readonly HandleVietQrWebhookCommandHandler _handler;

    public HandleVietQrWebhookCommandHandlerTests()
    {
        _clockMock.SetupGet(c => c.UtcNow).Returns(_now);
        _flashSales.Setup(s => s.ConfirmSoldAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());
        _vietQrAdapterMock.Setup(v => v.VerifyWebhookSignature(It.IsAny<string>(), It.IsAny<string?>())).Returns(true);

        _handler = new HandleVietQrWebhookCommandHandler(
            _vietQrAdapterMock.Object,
            _paymentRepositoryMock.Object,
            _parentOrderRepositoryMock.Object,
            _subOrderRepositoryMock.Object,
            _shopWalletRepositoryMock.Object,
            _outboxRepositoryMock.Object,
            _clockMock.Object,
            NullLogger<HandleVietQrWebhookCommandHandler>.Instance, _flashSales.Object);
    }

    [Fact]
    public async Task Handle_WhenValidSuccessWebhook_UpdatesPaymentParentOrderSubOrderCreditsWalletAndOutbox()
    {
        // Arrange
        var shopId = Guid.NewGuid();
        var orderCode = "ORD-VQR-001";
        var amount = 300000m;
        var transId = "TXN_MB_98765";

        var request = new VietQrWebhookRequest
        {
            TransactionId = transId,
            Amount = amount,
            Content = $"NOVA {orderCode}",
            BankBrand = "MB",
            AccountNumber = "0000123456789"
        };

        _vietQrAdapterMock.Setup(v => v.ExtractOrderCode(request.Content)).Returns(orderCode);

        var parentOrder = CreateTestParentOrder(orderCode, amount, _now.AddMinutes(-10));
        var payment = Payment.Create(parentOrder.Id, PaymentMethod.VietQR, amount, _now.AddMinutes(-10));
        var subOrder = CreateTestSubOrder(parentOrder.Id, $"{orderCode}-1", shopId, amount, _now.AddMinutes(-10));

        _parentOrderRepositoryMock.Setup(p => p.GetByOrderCodeForUpdateAsync(orderCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(parentOrder);
        _paymentRepositoryMock.Setup(p => p.GetForUpdateAsync(parentOrder.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);
        _subOrderRepositoryMock.Setup(s => s.GetByParentIdsAsync(It.Is<IEnumerable<Guid>>(ids => ids.Contains(parentOrder.Id)), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync([subOrder]);

        var wallet = new ShopWallet(shopId);
        _shopWalletRepositoryMock.Setup(w => w.GetByShopIdForUpdateAsync(shopId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);

        // Act
        var result = await _handler.Handle(new HandleVietQrWebhookCommand(request, "{}", "valid_sig"), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _flashSales.Verify(f => f.ConfirmSoldAsync(It.Is<IEnumerable<Guid>>(ids => ids.SequenceEqual(new[] { subOrder.Id })), It.IsAny<CancellationToken>()), Times.Once);
        result.Value.Success.Should().BeTrue();
        result.Value.OrderCode.Should().Be(orderCode);

        payment.Status.Should().Be(PaymentStatus.Success);
        payment.PaidAt.Should().Be(_now);
        payment.TransactionRef.Should().Be(transId);

        parentOrder.PaymentStatus.Should().Be(ParentOrderPaymentStatus.Paid);
        subOrder.Status.Should().Be(SubOrderStatus.Confirmed);

        wallet.Balance.Should().Be(amount);
        _shopWalletRepositoryMock.Verify(w => w.AddTransactionAsync(
            It.Is<ShopWalletTransaction>(t => t.Type == WalletTxType.OrderRevenue && t.Amount == amount),
            It.IsAny<CancellationToken>()), Times.Once);

        _outboxRepositoryMock.Verify(o => o.AddAsync(
            It.Is<OutboxMessage>(m => m.EventType == "PaymentSuccessEvent"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenSignatureInvalid_ReturnsInvalidSignatureError()
    {
        // Arrange
        var request = new VietQrWebhookRequest
        {
            TransactionId = "TXN_001",
            Amount = 100000,
            Content = "NOVA ORD-001"
        };

        _vietQrAdapterMock.Setup(v => v.VerifyWebhookSignature(It.IsAny<string>(), "invalid_sig")).Returns(false);

        // Act
        var result = await _handler.Handle(new HandleVietQrWebhookCommand(request, "{}", "invalid_sig"), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PaymentErrors.InvalidSignature);
    }

    [Fact]
    public async Task Handle_WhenTransactionIdMissing_ReturnsMissingTransactionIdError()
    {
        // Arrange
        var request = new VietQrWebhookRequest
        {
            TransactionId = null,
            Id = null,
            ReferenceCode = null,
            Amount = 100000,
            Content = "NOVA ORD-001"
        };

        // Act
        var result = await _handler.Handle(new HandleVietQrWebhookCommand(request, "{}", "valid_sig"), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PaymentErrors.MissingTransactionId);
    }

    [Fact]
    public async Task Handle_WhenCannotParseOrderCode_ReturnsCannotParseOrderCodeError()
    {
        // Arrange
        var request = new VietQrWebhookRequest
        {
            TransactionId = "TXN_001",
            Amount = 100000,
            Content = "Chuyen tien ca fe"
        };

        _vietQrAdapterMock.Setup(v => v.ExtractOrderCode(request.Content)).Returns((string?)null);

        // Act
        var result = await _handler.Handle(new HandleVietQrWebhookCommand(request, "{}", "valid_sig"), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PaymentErrors.CannotParseOrderCode);
    }

    [Fact]
    public async Task Handle_WhenOrderNotFound_ReturnsOrderNotFoundError()
    {
        // Arrange
        var request = new VietQrWebhookRequest
        {
            TransactionId = "TXN_002",
            Amount = 100000,
            Content = "NOVA NON_EXISTENT"
        };

        _vietQrAdapterMock.Setup(v => v.ExtractOrderCode(request.Content)).Returns("NON_EXISTENT");
        _parentOrderRepositoryMock.Setup(p => p.GetByOrderCodeForUpdateAsync("NON_EXISTENT", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ParentOrder?)null);

        // Act
        var result = await _handler.Handle(new HandleVietQrWebhookCommand(request, "{}", "valid_sig"), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PaymentErrors.OrderNotFound);
    }

    [Fact]
    public async Task Handle_WhenPaymentAlreadySuccess_IsIdempotentAndReturnsSuccess()
    {
        // Arrange
        var orderCode = "ORD-VQR-PAID";
        var amount = 150000m;
        var request = new VietQrWebhookRequest
        {
            TransactionId = "TXN_DUP",
            Amount = amount,
            Content = $"NOVA {orderCode}"
        };

        _vietQrAdapterMock.Setup(v => v.ExtractOrderCode(request.Content)).Returns(orderCode);

        var parentOrder = CreateTestParentOrder(orderCode, amount, _now.AddMinutes(-30));
        var payment = Payment.Create(parentOrder.Id, PaymentMethod.VietQR, amount, _now.AddMinutes(-30));
        payment.MarkSuccess("TXN_PREV", _now.AddMinutes(-20));

        _parentOrderRepositoryMock.Setup(p => p.GetByOrderCodeForUpdateAsync(orderCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(parentOrder);
        _paymentRepositoryMock.Setup(p => p.GetForUpdateAsync(parentOrder.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        // Act
        var result = await _handler.Handle(new HandleVietQrWebhookCommand(request, "{}", "valid_sig"), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Success.Should().BeTrue();
        result.Value.Message.Should().Contain("already processed");

        _shopWalletRepositoryMock.Verify(w => w.AddTransactionAsync(It.IsAny<ShopWalletTransaction>(), It.IsAny<CancellationToken>()), Times.Never);
        _outboxRepositoryMock.Verify(o => o.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenAmountLessThanRequired_ReturnsInvalidAmountError()
    {
        // Arrange
        var orderCode = "ORD-VQR-UNDERPAID";
        var requiredAmount = 200000m;
        var transferredAmount = 150000m; // Thiếu 50,000

        var request = new VietQrWebhookRequest
        {
            TransactionId = "TXN_UNDER",
            Amount = transferredAmount,
            Content = $"NOVA {orderCode}"
        };

        _vietQrAdapterMock.Setup(v => v.ExtractOrderCode(request.Content)).Returns(orderCode);

        var parentOrder = CreateTestParentOrder(orderCode, requiredAmount, _now.AddMinutes(-10));
        var payment = Payment.Create(parentOrder.Id, PaymentMethod.VietQR, requiredAmount, _now.AddMinutes(-10));

        _parentOrderRepositoryMock.Setup(p => p.GetByOrderCodeForUpdateAsync(orderCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(parentOrder);
        _paymentRepositoryMock.Setup(p => p.GetForUpdateAsync(parentOrder.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        // Act
        var result = await _handler.Handle(new HandleVietQrWebhookCommand(request, "{}", "valid_sig"), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PaymentErrors.InvalidAmount);
        payment.Status.Should().Be(PaymentStatus.Pending);
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
