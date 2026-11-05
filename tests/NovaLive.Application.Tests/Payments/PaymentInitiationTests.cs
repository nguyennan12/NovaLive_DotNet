using FluentAssertions;
using Moq;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Abstractions.ThirdParty;
using NovaLive.Application.UseCases.Payments.Commands.InitiatePayment;
using NovaLive.Application.UseCases.Payments.Queries.GetPaymentStatus;
using NovaLive.Contracts.V1.Payments;
using NovaLive.Domain.Common;
using NovaLive.Domain.Orders;
using NovaLive.Domain.Payments;

namespace NovaLive.Application.Tests.Payments;

public sealed class PaymentInitiationTests
{
    private readonly DateTimeOffset now = DateTimeOffset.Parse("2026-11-02T00:00:00Z");
    private readonly Guid buyer = Guid.NewGuid();
    private readonly Mock<ICurrentUser> user = new();
    private readonly Mock<IParentOrderRepository> orders = new();
    private readonly Mock<IPaymentRepository> payments = new();
    private readonly Mock<IMomoPaymentAdapter> momo = new(MockBehavior.Strict);
    private readonly Mock<IVietQrPaymentAdapter> qr = new(MockBehavior.Strict);
    private readonly Mock<IDateTimeProvider> clock = new();
    private ParentOrder order = null!;
    private Payment payment = null!;

    private void Seed(PaymentMethod method, int age = 5)
    {
        user.SetupGet(u => u.UserId).Returns(buyer);
        clock.SetupGet(c => c.UtcNow).Returns(now);
        order = ParentOrder.Create(buyer, "ORD-123", "{}", 100000, 0, 0, 100000, null, now.AddMinutes(-age));
        payment = Payment.Create(order.Id, method, order.GrandTotal, now.AddMinutes(-age));
        orders.Setup(o => o.FindAsync(order.Id, buyer, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        payments.Setup(p => p.GetByParentOrderIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(payment);
        payments.Setup(p => p.GetByIdAsync(payment.Id, It.IsAny<CancellationToken>())).ReturnsAsync(payment);
    }

    private Task<Result<PaymentInitResponse>> Initiate(string? method = null) =>
        new InitiatePaymentCommandHandler(user.Object, orders.Object, payments.Object, momo.Object, qr.Object, clock.Object)
            .Handle(new(new(order.Id, method ?? payment.Method.ToString())), default);

    [Fact]
    public async Task Momo_UsesServerAmountAndOrderCode_AndKeepsOriginalExpiryOnRetry()
    {
        Seed(PaymentMethod.MoMo);
        momo.Setup(m => m.CreatePaymentUrlAsync(order.OrderCode, 100000, It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MomoCreatePaymentResponse { PayUrl = "https://pay.momo.vn/123" });
        var first = await Initiate();
        clock.SetupGet(c => c.UtcNow).Returns(now.AddMinutes(2));
        var retry = await Initiate();
        first.Value!.PaymentUrl.Should().Be("https://pay.momo.vn/123");
        retry.Value!.ExpiredAt.Should().Be(payment.CreatedAt.AddMinutes(15));
        payment.Status.Should().Be(PaymentStatus.Pending);
        payments.Verify(p => p.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task VietQr_ReturnsPayloadAndBase64()
    {
        Seed(PaymentMethod.VietQR);
        qr.Setup(q => q.GenerateDynamicQrAsync(order.OrderCode, 100000, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VietQrResponse("payload", "url", "name", "account", "bin", 100000, "NOVA ORD-123", "base64"));
        var result = await Initiate("VietQR");
        result.Value!.QrPayload.Should().Be("payload");
        result.Value!.QrImageBase64.Should().Be("base64");
    }

    [Fact]
    public async Task Cod_DoesNotCallOnlineAdaptersAndNeverExpires()
    {
        Seed(PaymentMethod.COD, 10000);
        var result = await Initiate();
        result.Value!.Status.Should().Be("Pending");
        result.Value!.ExpiredAt.Should().BeNull();
        result.Value!.PaymentUrl.Should().BeNull();
        result.Value!.QrPayload.Should().BeNull();
        payment.Status.Should().Be(PaymentStatus.Pending);
    }

    [Theory]
    [InlineData(15)]
    [InlineData(16)]
    public async Task ExpiredOnlinePayment_IsRejectedBeforeCallingProvider(int age)
    {
        Seed(PaymentMethod.MoMo, age);
        (await Initiate()).Error.Code.Should().Be("Payment.Expired");
    }

    [Theory]
    [InlineData("COD")]
    [InlineData("VietQR")]
    [InlineData("0")]
    [InlineData("unsupported")]
    public async Task ChangingCheckoutMethodOrInvalidMethod_IsRejected(string method)
    {
        Seed(PaymentMethod.MoMo);
        (await Initiate(method)).Error.Code.Should().Be("Payment.InvalidMethod");
    }

    [Fact]
    public async Task AnotherBuyer_CannotInitiateOrPoll()
    {
        Seed(PaymentMethod.MoMo);
        user.SetupGet(u => u.UserId).Returns(Guid.NewGuid());
        (await Initiate()).Error.Code.Should().Be("Payment.OrderNotFound");
        var status = await new GetPaymentStatusQueryHandler(user.Object, payments.Object, orders.Object).Handle(new(payment.Id), default);
        status.Error.Code.Should().Be("Payment.NotFound");
    }

    [Fact]
    public async Task PaidOrFailedPayments_AreRejected()
    {
        Seed(PaymentMethod.MoMo);
        payment.MarkFailed("{}", now);
        (await Initiate()).Error.Code.Should().Be("Payment.NotPending");
        Seed(PaymentMethod.MoMo);
        order.MarkPaid(now);
        (await Initiate()).Error.Code.Should().Be("Payment.NotPending");
    }

    [Fact]
    public async Task MomoRejectionOrMissingUrl_IsNotReportedAsSuccessfulInitiation()
    {
        Seed(PaymentMethod.MoMo);
        momo.Setup(m => m.CreatePaymentUrlAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MomoCreatePaymentResponse { ResultCode = 1006, Message = "rejected" });
        (await Initiate()).Error.Code.Should().Be("Payment.GatewayFailed");
    }

    [Fact]
    public async Task GatewayNetworkError_IsRetryableAndDoesNotChangePayment()
    {
        Seed(PaymentMethod.MoMo);
        momo.Setup(m => m.CreatePaymentUrlAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException());
        (await Initiate()).Error.Code.Should().Be("Payment.GatewayFailed");
        payment.Status.Should().Be(PaymentStatus.Pending);
    }

    [Fact]
    public async Task Polling_ReflectsPendingThenSuccessFromDatabase()
    {
        Seed(PaymentMethod.MoMo);
        var handler = new GetPaymentStatusQueryHandler(user.Object, payments.Object, orders.Object);
        (await handler.Handle(new(payment.Id), default)).Value!.Status.Should().Be("Pending");
        payment.MarkSuccess("TX", now);
        var paid = (await handler.Handle(new(payment.Id), default)).Value!;
        paid.Status.Should().Be("Success");
        paid.PaidAt.Should().Be(now);
    }

    [Theory]
    [InlineData("0", false)]
    [InlineData("MoMo", true)]
    [InlineData("VietQR", true)]
    [InlineData("COD", true)]
    public void Validator_RejectsNumericEnumValues(string method, bool valid)
    {
        new InitiatePaymentCommandValidator().Validate(new InitiatePaymentCommand(new(Guid.NewGuid(), method))).IsValid.Should().Be(valid);
    }
}
