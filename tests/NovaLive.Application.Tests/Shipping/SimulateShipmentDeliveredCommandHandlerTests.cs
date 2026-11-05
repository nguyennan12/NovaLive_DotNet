using System.Text.Json;
using FluentAssertions;
using NovaLive.Application.UseCases.Shipping;
using NovaLive.Application.UseCases.Shipping.Commands.SimulateShipmentDelivered;
using NovaLive.Contracts.V1.Shipping;
using NovaLive.Domain.Common;
using NovaLive.Domain.Orders;
using Xunit;

namespace NovaLive.Application.Tests.Shipping;

public sealed class SimulateShipmentDeliveredCommandHandlerTests
{
    [Fact]
    public async Task OwnedGhnShipment_MarksShipmentAndOrderDeliveredWithoutVerifiedCodSignal()
    {
        var c = await ShippingFulfillmentTestContext.Build();
        (await c.Create()).IsSuccess.Should().BeTrue();

        var result = await c.SimulateHandler.Handle(new SimulateShipmentDeliveredCommand(c.Sub.Id), default);

        result.IsSuccess.Should().BeTrue();
        c.Saved.Single().Status.Should().Be(ShippingStatus.Delivered);
        c.Sub.Status.Should().Be(SubOrderStatus.Delivered);
        var delivered = c.Orders.Checkout.Messages
            .Where(message => message.EventType == nameof(ShipmentDeliveredEvent))
            .Select(message => JsonSerializer.Deserialize<ShipmentDeliveredEvent>(message.Payload,
                new JsonSerializerOptions(JsonSerializerDefaults.Web)))
            .Should().ContainSingle().Subject;
        delivered.Should().NotBeNull();
        delivered!.IsVerifiedWebhook.Should().BeFalse();
        c.Orders.Checkout.HistoryList.Should().ContainSingle(history =>
            history.ToStatus == nameof(SubOrderStatus.Delivered)
            && history.Note == "Demo delivery confirmed by seller");
    }

    [Fact]
    public async Task RepeatedSimulation_IsIdempotent()
    {
        var c = await ShippingFulfillmentTestContext.Build();
        await c.Create();

        (await c.SimulateHandler.Handle(new(c.Sub.Id), default)).IsSuccess.Should().BeTrue();
        (await c.SimulateHandler.Handle(new(c.Sub.Id), default)).IsSuccess.Should().BeTrue();

        c.Orders.Checkout.Messages.Count(message => message.EventType == nameof(ShipmentDeliveredEvent)).Should().Be(1);
        c.Orders.Checkout.HistoryList.Count(history => history.ToStatus == nameof(SubOrderStatus.Delivered)).Should().Be(1);
    }

    [Fact]
    public async Task MissingOrForeignOrder_ReturnsNotFound()
    {
        var c = await ShippingFulfillmentTestContext.Build();

        var result = await c.SimulateHandler.Handle(new(Guid.NewGuid()), default);

        result.Error.Should().Be(ShippingErrors.SubOrderNotFound);
        c.Orders.Checkout.Messages.Should().BeEmpty();
    }

    [Fact]
    public async Task ConfirmedOrderWithoutShipment_IsRejected()
    {
        var c = await ShippingFulfillmentTestContext.Build();

        var result = await c.SimulateHandler.Handle(new(c.Sub.Id), default);

        result.Error.Should().Be(ShippingErrors.ShipmentNotFound);
        c.Sub.Status.Should().Be(SubOrderStatus.Confirmed);
    }
}
