using MassTransit;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.UseCases.Orders;
using NovaLive.Contracts.V1.Orders;
using NovaLive.Infrastructure.System;

namespace NovaLive.Infrastructure.Tests.System;

public sealed class OrderPlacedConsumerTests
{
    [Fact]
    public async Task Consume_WhenEventWasAlreadyClaimed_SkipsAllSideEffects()
    {
        var idempotency = new Mock<IIdempotencyService>();
        idempotency.Setup(service => service.TryAcquireAsync("order-placed:key", It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var users = new Mock<IUserRepository>(MockBehavior.Strict);
        var shops = new Mock<IShopRepository>(MockBehavior.Strict);
        var notifications = new Mock<INotificationRepository>(MockBehavior.Strict);
        var email = new Mock<IOrderConfirmationEmailSender>(MockBehavior.Strict);
        var orderNotification = new Mock<IOrderNotificationService>(MockBehavior.Strict);
        var consumer = new OrderPlacedConsumer(users.Object, shops.Object, notifications.Object, email.Object,
            orderNotification.Object, idempotency.Object, NullLogger<OrderPlacedConsumer>.Instance);
        var context = new Mock<ConsumeContext<OrderPlacedEvent>>();
        context.SetupGet(item => item.Message).Returns(CreateEvent());

        await consumer.Consume(context.Object);

        users.VerifyNoOtherCalls();
        shops.VerifyNoOtherCalls();
        notifications.VerifyNoOtherCalls();
        email.VerifyNoOtherCalls();
        orderNotification.VerifyNoOtherCalls();
    }

    [Fact]
    public void EmailTemplate_EncodesBuyerControlledTextAndIncludesOrderLines()
    {
        var html = OrderPlacedEmailTemplate.Render(CreateEvent() with
        {
            OrderCode = "<order>",
            SubOrders = [new OrderPlacedShop(Guid.NewGuid(), "NOVA-S1", Guid.NewGuid(), 1, 125m,
                [new OrderPlacedItem("<product>", "SKU-1", null, 1, 125m, 125m)])]
        });

        html.Should().Contain("&lt;order&gt;").And.Contain("&lt;product&gt;").And.Contain("SKU-1");
        html.Should().NotContain("<product>");
    }

    private static OrderPlacedEvent CreateEvent() => new(
        Guid.NewGuid(), "NOVA-20261018-0001", DateTimeOffset.UtcNow, Guid.NewGuid(), [], "COD", 100m, "key",
        new CheckoutResponse(Guid.NewGuid(), "NOVA-20261018-0001", 100m, "COD", Guid.NewGuid(), "Pending", [], null));
}
