using FluentAssertions;
using MassTransit;
using MediatR;
using Moq;
using NovaLive.Application.UseCases.Inventory;
using NovaLive.Application.UseCases.Inventory.Commands.ConfirmInventorySale;
using NovaLive.Contracts.V1.Payments;
using NovaLive.Domain.Common;
using NovaLive.Infrastructure.System;

namespace NovaLive.Infrastructure.Tests.System;

public sealed class InventorySyncConsumerTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Consumer_MapsOnlyPaymentIdAndCancellation_AndPropagatesFailureToTransport(bool success)
    {
        var paymentId = Guid.NewGuid();
        using var cancellation = new CancellationTokenSource();
        var sender = new Mock<ISender>();
        sender.Setup(s => s.Send(It.Is<ConfirmInventorySaleCommand>(c => c.PaymentId == paymentId), cancellation.Token))
            .ReturnsAsync(success ? Result.Success() : Result.Failure(InventoryErrors.ReservationMismatch));
        var context = new Mock<ConsumeContext<PaymentSuccessEvent>>();
        context.SetupGet(c => c.Message).Returns(new PaymentSuccessEvent(paymentId, Guid.NewGuid(), "NOVA", 100,
            "MoMo", "ref", new DateTimeOffset(2026, 10, 21, 0, 0, 0, TimeSpan.Zero)));
        context.SetupGet(c => c.CancellationToken).Returns(cancellation.Token);
        var consumer = new InventorySyncConsumer(sender.Object);
        if (success) await consumer.Consume(context.Object);
        else await FluentActions.Awaiting(() => consumer.Consume(context.Object)).Should()
            .ThrowAsync<InvalidOperationException>().WithMessage("*Inventory.ReservationMismatch*");
        sender.VerifyAll();
    }
}
