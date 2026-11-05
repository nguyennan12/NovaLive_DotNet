using MassTransit;
using Moq;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Contracts.V1.Payments;
using NovaLive.Infrastructure.System;

namespace NovaLive.Infrastructure.Tests.Payments;

public sealed class PaymentSuccessConsumerTests
{
    private static Mock<ConsumeContext<PaymentSuccessEvent>> Context()
    {
        var context = new Mock<ConsumeContext<PaymentSuccessEvent>>();
        context.SetupGet(c => c.Message).Returns(new PaymentSuccessEvent(Guid.NewGuid(), Guid.NewGuid(),
            "NOVA-1", 100, "VietQR", "TX", DateTimeOffset.UtcNow));
        return context;
    }

    [Fact]
    public async Task DuplicateEvent_DoesNotNotifyTwice()
    {
        var context = Context();
        var notifications = new Mock<IPaymentNotificationService>();
        var idempotency = new Mock<IIdempotencyService>();
        idempotency.SetupSequence(i => i.TryAcquireAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true).ReturnsAsync(false);
        var consumer = new PaymentSuccessConsumer(notifications.Object, idempotency.Object);
        await consumer.Consume(context.Object);
        await consumer.Consume(context.Object);
        notifications.Verify(n => n.NotifyPaymentSuccessAsync(context.Object.Message, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task FailedNotification_ReleasesLeaseAndAllowsRetry()
    {
        var context = Context();
        var notifications = new Mock<IPaymentNotificationService>();
        notifications.Setup(n => n.NotifyPaymentSuccessAsync(It.IsAny<PaymentSuccessEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("SignalR unavailable"));
        var idempotency = new Mock<IIdempotencyService>();
        idempotency.Setup(i => i.TryAcquireAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new PaymentSuccessConsumer(notifications.Object, idempotency.Object).Consume(context.Object));
        idempotency.Verify(i => i.RemoveAsync($"payment-success:{context.Object.Message.PaymentId:N}", It.IsAny<CancellationToken>()), Times.Once);
        idempotency.Verify(i => i.CreateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
