using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Common.Behaviors;
using NovaLive.Application.UseCases.Shipping;
using NovaLive.Application.UseCases.Shipping.Commands.Webhook;
using NovaLive.Contracts.V1.Shipping;
using NovaLive.Domain.Common;
using Xunit;

namespace NovaLive.Application.Tests.Shipping;

public sealed class HandleShippingWebhookCommandHandlerTests
{
    [Fact]
    public async Task UnknownTracking_ReturnsNotFoundWithoutWrites()
    {
        var c = new ShippingWebhookTestContext();
        var result = await c.Handler.Handle(c.Command() with { TrackingCode = "missing" }, default);
        result.Error.Should().Be(ShippingErrors.OrderNotFound);
        c.Messages.Should().BeEmpty(); c.HistoryList.Should().BeEmpty();
        c.Shipment.Status.Should().Be(ShippingStatus.ReadyToPick);
    }

    [Fact]
    public async Task Delivered_StoresClockTimeSystemHistoryAndOutboxExactlyOnce()
    {
        var c = new ShippingWebhookTestContext();
        (await c.Handler.Handle(c.Command(), default)).IsSuccess.Should().BeTrue();
        c.Clock.SetupGet(t => t.UtcNow).Returns(c.Now.AddDays(1));
        (await c.Handler.Handle(c.Command() with { EventTimestamp = c.Now.AddDays(1) }, default)).IsSuccess.Should().BeTrue();
        c.Shipment.Status.Should().Be(ShippingStatus.Delivered);
        c.Shipment.DeliveredAt.Should().Be(c.Now);
        c.Sub.Status.Should().Be(SubOrderStatus.Delivered); c.Sub.DeliveredAt.Should().Be(c.Now);
        var history = c.HistoryList.Should().ContainSingle().Subject;
        history.FromStatus.Should().Be("Shipping"); history.ToStatus.Should().Be("Delivered");
        history.ChangedByRole.Should().Be(OrderRole.System); history.ChangedBy.Should().BeNull();
        history.ChangedAt.Should().Be(c.Now);
        var message = c.Messages.Should().ContainSingle().Subject;
        message.EventType.Should().Be(nameof(ShipmentDeliveredEvent)); message.CreatedAt.Should().Be(c.Now);
        JsonSerializer.Deserialize<ShipmentDeliveredEvent>(message.Payload, new JsonSerializerOptions(JsonSerializerDefaults.Web))
            .Should().Be(new ShipmentDeliveredEvent(c.Shipment.Id, c.Sub.Id, c.Sub.ParentOrderId, c.Sub.ShopId, "GHN", "TRACK-1", c.Now));
    }

    [Fact]
    public async Task OldAndDuplicateProgress_DoNotRegressOrChangePayloadOrUpdatedTime()
    {
        var c = new ShippingWebhookTestContext();
        await c.Handler.Handle(c.Command(ShippingStatus.Delivering), default);
        c.Clock.SetupGet(t => t.UtcNow).Returns(c.Now.AddHours(1));
        await c.Handler.Handle(c.Command(ShippingStatus.Picking) with { RawPayload = "old" }, default);
        await c.Handler.Handle(c.Command(ShippingStatus.Delivering) with { RawPayload = "duplicate" }, default);
        c.Shipment.Status.Should().Be(ShippingStatus.Delivering); c.Shipment.UpdatedAt.Should().Be(c.Now);
        c.Shipment.WebhookPayload.Should().Be("{}"); c.Sub.Status.Should().Be(SubOrderStatus.Shipping);
        c.Messages.Should().BeEmpty(); c.HistoryList.Should().BeEmpty();
    }

    [Theory]
    [InlineData(ShippingStatus.Failed)]
    [InlineData(ShippingStatus.Returned)]
    [InlineData(ShippingStatus.Cancelled)]
    public async Task CarrierFailure_OnlyUpdatesShipment(ShippingStatus status)
    {
        var c = new ShippingWebhookTestContext();
        await c.Handler.Handle(c.Command(status), default);
        c.Shipment.Status.Should().Be(status); c.Sub.Status.Should().Be(SubOrderStatus.Shipping);
        c.Sub.DeliveredAt.Should().BeNull(); c.Messages.Should().BeEmpty(); c.HistoryList.Should().BeEmpty();
    }

    [Theory]
    [InlineData(ShippingStatus.Failed)]
    [InlineData(ShippingStatus.Returned)]
    [InlineData(ShippingStatus.Cancelled)]
    public async Task CarrierExceptionAfterDelivery_DoesNotUndoSubOrderOrDeliverAgain(ShippingStatus status)
    {
        var c = new ShippingWebhookTestContext();
        await c.Handler.Handle(c.Command(), default);
        await c.Handler.Handle(c.Command(status), default);
        await c.Handler.Handle(c.Command() with { EventTimestamp = c.Now.AddHours(1) }, default);
        c.Shipment.Status.Should().Be(status); c.Sub.Status.Should().Be(SubOrderStatus.Delivered);
        c.Sub.DeliveredAt.Should().Be(c.Now); c.Messages.Should().ContainSingle(); c.HistoryList.Should().ContainSingle();
    }

    [Fact]
    public async Task RedisIdempotency_ReplaysSuccessWithoutInvokingHandlerAgain()
    {
        var c = new ShippingWebhookTestContext(); var calls = 0; Result? saved = null;
        var redis = new Mock<IIdempotencyService>(); var command = c.Command();
        redis.Setup(r => r.GetResponseAsync<Result>(command.IdempotencyKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => saved);
        redis.Setup(r => r.SaveResponseAsync(command.IdempotencyKey, It.IsAny<Result>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
            .Callback((string _, Result response, TimeSpan? _, CancellationToken _) => saved = response).Returns(Task.CompletedTask);
        var pipeline = new IdempotencyBehavior<HandleShippingWebhookCommand, Result>(redis.Object,
            NullLogger<IdempotencyBehavior<HandleShippingWebhookCommand, Result>>.Instance);
        async Task<Result> Next(CancellationToken ct) { calls++; return await c.Handler.Handle(command, ct); }
        (await pipeline.Handle(command, Next, default)).IsSuccess.Should().BeTrue();
        (await pipeline.Handle(command, Next, default)).IsSuccess.Should().BeTrue();
        calls.Should().Be(1); c.Messages.Should().ContainSingle(); c.HistoryList.Should().ContainSingle();
    }
}
