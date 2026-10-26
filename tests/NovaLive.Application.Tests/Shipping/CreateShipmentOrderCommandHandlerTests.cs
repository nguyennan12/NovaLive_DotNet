using System.Text.Json;
using FluentAssertions;
using Moq;
using NovaLive.Application.Abstractions.ThirdParty;
using NovaLive.Application.UseCases.Shipping;
using NovaLive.Application.UseCases.Shipping.Commands.CreateShipmentOrder;
using NovaLive.Contracts.V1.Shipping;
using NovaLive.Domain.Common;

namespace NovaLive.Application.Tests.Shipping;

public sealed class CreateShipmentOrderCommandHandlerTests
{
    [Fact]
    public async Task Success_PersistsSnapshotsWeightCodHistoryAndOutbox_ThenRejectsRetry()
    {
        var c = await ShippingFulfillmentTestContext.Build();
        ShippingShipmentCreateModel? sent = null;
        c.Provider.Setup(p => p.CreateShipmentAsync(It.IsAny<ShippingShipmentCreateModel>(), It.IsAny<CancellationToken>()))
            .Callback((ShippingShipmentCreateModel m, CancellationToken _) => sent = m)
            .ReturnsAsync(new ShippingOrderCreateResult("PROVIDER-1", "TRACK-1", 32000m));
        var result = await c.Create();
        result.IsSuccess.Should().BeTrue();
        c.Sub.Status.Should().Be(SubOrderStatus.Shipping);
        c.Sub.ShippedAt.Should().Be(c.Orders.Checkout.Draft.Now);
        var shipment = c.Saved.Should().ContainSingle().Subject;
        shipment.Status.Should().Be(ShippingStatus.ReadyToPick);
        shipment.ProviderOrderId.Should().Be("PROVIDER-1");
        shipment.TrackingCode.Should().Be("TRACK-1");
        shipment.ShippingFee.Should().Be(32000m);
        shipment.DeliveryAddressJson.Should().Be(c.Orders.Parent.ShippingAddressJson);
        using var pickup = JsonDocument.Parse(shipment.PickupAddressJson);
        pickup.RootElement.GetProperty("contactName").GetString().Should().Be(c.Orders.Checkout.Draft.WarehouseList[0].ContactName);
        var expectedWeight = c.Orders.Checkout.ItemList.Where(i => i.SubOrderId == c.Sub.Id)
            .Sum(i => c.Orders.Checkout.Draft.SkuList.Single(s => s.Id == i.SkuId).WeightGram * i.Quantity);
        shipment.WeightGram.Should().Be(expectedWeight);
        sent!.WeightGram.Should().Be(expectedWeight);
        sent.Items.Sum(i => i.Quantity * i.WeightGram).Should().Be(expectedWeight);
        sent.CodAmount.Should().Be(c.Sub.SubTotal); shipment.CodAmount.Should().Be(c.Sub.SubTotal);
        sent.ServiceTypeId.Should().Be(2); sent.ServiceCode.Should().Be("2");
        sent.ToAddress.Should().Be(c.Orders.Checkout.Draft.Address.DetailAddress);
        c.Orders.Checkout.HistoryList.Should().ContainSingle(h => h.FromStatus == "Confirmed"
            && h.ToStatus == "Shipping" && h.ChangedByRole == OrderRole.Seller && h.ChangedBy == c.Orders.SellerId);
        var message = c.Orders.Checkout.Messages.Should().ContainSingle().Subject;
        message.EventType.Should().Be(nameof(ShipmentCreatedEvent));
        message.CreatedAt.Should().Be(c.Orders.Checkout.Draft.Now);
        message.Payload.Should().Contain("TRACK-1");
        (await c.Create()).Error.Should().Be(ShippingErrors.ShipmentAlreadyCreated);
        c.Provider.Verify(p => p.CreateShipmentAsync(It.IsAny<ShippingShipmentCreateModel>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task OnlinePaidOrder_HasZeroCod()
    {
        var c = await ShippingFulfillmentTestContext.Build(cod: false);
        c.Sub.ConfirmPayment(c.Orders.Checkout.Draft.Now);
        (await c.Create()).IsSuccess.Should().BeTrue();
        c.Saved.Single().CodAmount.Should().Be(0);
    }

    [Fact]
    public async Task MultipleSkus_UseEachWeightTimesQuantity()
    {
        var c = await ShippingFulfillmentTestContext.Build();
        var sku = new NovaLive.Domain.Products.Sku(Guid.NewGuid(), c.Sub.ShopId, "SKU2", null, 100, 100, 725);
        c.Orders.Checkout.Draft.SkuList.Add(sku);
        var price = new NovaLive.Domain.Pricing.OrderItemCalculationResult(sku.Id, sku.SpuId, 3, 100, 300, 0, 0, 0, 300);
        c.Orders.Checkout.ItemList.Add(NovaLive.Domain.Orders.OrderItem.Create(c.Sub.Id,
            """{"skuCode":"SKU2","spuName":"Product 2"}""", 100, price));
        var expected = c.Orders.Checkout.ItemList.Sum(i =>
            c.Orders.Checkout.Draft.SkuList.Single(s => s.Id == i.SkuId).WeightGram * i.Quantity);
        (await c.Create()).IsSuccess.Should().BeTrue();
        c.Saved.Single().WeightGram.Should().Be(expected);
        c.Provider.Verify(p => p.CreateShipmentAsync(It.Is<ShippingShipmentCreateModel>(m =>
            m.Items.Count == 2 && m.WeightGram == expected), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ForeignSubOrderAndSpoofedShopContext_AreHidden()
    {
        var c = await ShippingFulfillmentTestContext.Build(shops: 2);
        var foreign = c.Orders.Checkout.SubList.Single(s => s.ShopId != c.Orders.HeaderShopId);
        (await c.CreateHandler.Handle(new(c.Request with { SubOrderId = foreign.Id }), default))
            .Error.Should().Be(ShippingErrors.SubOrderNotFound);
        c.Orders.HeaderShopId = foreign.ShopId;
        (await c.CreateHandler.Handle(new(c.Request with { SubOrderId = foreign.Id }), default))
            .Error.Should().Be(ShippingErrors.SubOrderNotFound);
        c.Provider.Verify(p => p.CreateShipmentAsync(It.IsAny<ShippingShipmentCreateModel>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(SubOrderStatus.PendingPayment)]
    [InlineData(SubOrderStatus.Processing)]
    [InlineData(SubOrderStatus.Shipping)]
    [InlineData(SubOrderStatus.Delivered)]
    [InlineData(SubOrderStatus.Cancelled)]
    public async Task UnconfirmedOrder_DoesNotCallProvider(SubOrderStatus state)
    {
        var c = await ShippingFulfillmentTestContext.Build();
        typeof(NovaLive.Domain.Orders.SubOrder).GetProperty("Status")!.SetValue(c.Sub, state);
        (await c.Create()).Error.Should().Be(ShippingErrors.InvalidOrderStatus);
        c.Saved.Should().BeEmpty();
        c.Provider.Verify(p => p.CreateShipmentAsync(It.IsAny<ShippingShipmentCreateModel>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ForeignWarehouse_IsRejected()
    {
        var c = await ShippingFulfillmentTestContext.Build(shops: 2);
        var foreign = c.Orders.Checkout.Draft.WarehouseList.Single(w => w.ShopId != c.Sub.ShopId);
        (await c.CreateHandler.Handle(new(c.Request with { PickupAddressId = foreign.Id }), default))
            .Error.Should().Be(ShippingErrors.InvalidShopWarehouse);
        c.Saved.Should().BeEmpty(); c.Sub.Status.Should().Be(SubOrderStatus.Confirmed);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ProviderFailureOrException_LeavesAllEntitiesUntouched(bool throws)
    {
        var c = await ShippingFulfillmentTestContext.Build();
        var setup = c.Provider.Setup(p => p.CreateShipmentAsync(It.IsAny<ShippingShipmentCreateModel>(), It.IsAny<CancellationToken>()));
        if (throws) setup.ThrowsAsync(new HttpRequestException("offline"));
        else setup.ReturnsAsync(ShippingErrors.ProviderError("GHN.Error", "offline"));
        var result = await c.Create();
        result.Error.Code.Should().Be("Shipping.ProviderError");
        c.Sub.Status.Should().Be(SubOrderStatus.Confirmed); c.Sub.ShippedAt.Should().BeNull();
        c.Saved.Should().BeEmpty(); c.Orders.Checkout.HistoryList.Should().BeEmpty(); c.Orders.Checkout.Messages.Should().BeEmpty();
    }

    [Fact]
    public async Task UnavailableProvider_ReturnsResultWithoutCallingCarrier()
    {
        var c = await ShippingFulfillmentTestContext.Build();
        c.Resolver.Setup(r => r.Resolve("GHTK")).Returns(ShippingErrors.ProviderNotAvailable);
        (await c.CreateHandler.Handle(new(c.Request with { Provider = "GHTK" }), default))
            .Error.Should().Be(ShippingErrors.ProviderNotAvailable);
        c.Sub.Status.Should().Be(SubOrderStatus.Confirmed); c.Saved.Should().BeEmpty();
    }

    [Fact]
    public async Task MissingWarehouse_IsAssignedOnlyAfterSuccess()
    {
        var c = await ShippingFulfillmentTestContext.Build();
        typeof(NovaLive.Domain.Orders.SubOrder).GetProperty("WarehouseAddressId")!.SetValue(c.Sub, null);
        (await c.Create()).IsSuccess.Should().BeTrue();
        c.Sub.WarehouseAddressId.Should().Be(c.Request.PickupAddressId);
    }

    [Fact]
    public async Task MalformedSnapshot_IsValidationFailure()
    {
        var c = await ShippingFulfillmentTestContext.Build();
        typeof(NovaLive.Domain.Orders.ParentOrder).GetProperty("ShippingAddressJson")!.SetValue(c.Orders.Parent, "{");
        (await c.Create()).Error.Should().Be(ShippingErrors.IncompleteRecipientAddress);
        c.Sub.Status.Should().Be(SubOrderStatus.Confirmed); c.Saved.Should().BeEmpty();
    }
}
