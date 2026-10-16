using FluentAssertions;
using Moq;
using NovaLive.Application.UseCases.Shipping;
using NovaLive.Domain.Common;
using NovaLive.Domain.Shipping;

namespace NovaLive.Application.Tests.Shipping;

public sealed class GetOrderTrackingQueryHandlerTests
{
    [Fact]
    public async Task BuyerOwnsParent_GetsOnlyShipmentsForItsSubOrders()
    {
        var c = await ShippingFulfillmentTestContext.Build(shops: 2); await c.Create();
        c.Saved.Add(new ShippingOrder(Guid.NewGuid(), ShippingProvider.GHN, "2", "{}", "{}", 0, 1, 100, "foreign"));
        c.Orders.Seller.SetupGet(u => u.UserId).Returns(c.Orders.Parent.BuyerId);
        var result = await c.TrackingHandler.Handle(new(c.Orders.Parent.Id), default);
        result.IsSuccess.Should().BeTrue();
        var row = result.Value.Should().ContainSingle().Subject;
        row.SubOrderId.Should().Be(c.Sub.Id); row.Status.Should().Be("delivering");
        row.IsStale.Should().BeFalse(); row.Events.Should().ContainSingle();
    }

    [Fact]
    public async Task BuyerCannotReadAnotherBuyersParent()
    {
        var c = await ShippingFulfillmentTestContext.Build(); await c.Create();
        (await c.TrackingHandler.Handle(new(c.Orders.Parent.Id), default)).Error.Should().Be(ShippingErrors.OrderNotFound);
        c.Provider.Verify(p => p.GetTrackingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SellerOwnsSubOrder_GetsLiveTracking()
    {
        var c = await ShippingFulfillmentTestContext.Build(); await c.Create();
        var result = await c.TrackingHandler.Handle(new(c.Sub.Id, true), default);
        result.Value.Should().ContainSingle().Which.IsStale.Should().BeFalse();
    }

    [Fact]
    public async Task SellerCannotReadForeignSubOrder()
    {
        var c = await ShippingFulfillmentTestContext.Build(shops: 2);
        var foreign = c.Orders.Checkout.SubList.Single(s => s.ShopId != c.Sub.ShopId);
        (await c.TrackingHandler.Handle(new(foreign.Id, true), default)).Error.Should().Be(ShippingErrors.SubOrderNotFound);
        c.Provider.Verify(p => p.GetTrackingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CarrierFailureOrException_FallsBackToPersistedStatus(bool throws)
    {
        var c = await ShippingFulfillmentTestContext.Build(); await c.Create();
        var setup = c.Provider.Setup(p => p.GetTrackingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()));
        if (throws) setup.ThrowsAsync(new HttpRequestException("offline"));
        else setup.ReturnsAsync(ShippingErrors.ProviderError("GHN.Error", "offline"));
        var result = await c.TrackingHandler.Handle(new(c.Sub.Id, true), default);
        result.IsSuccess.Should().BeTrue();
        var row = result.Value.Should().ContainSingle().Subject;
        row.Status.Should().Be("ReadyToPick"); row.IsStale.Should().BeTrue(); row.Events.Should().BeEmpty();
    }

    [Fact]
    public async Task BuyerWithoutShipments_GetsEmptyList()
    {
        var c = await ShippingFulfillmentTestContext.Build();
        c.Orders.Seller.SetupGet(u => u.UserId).Returns(c.Orders.Parent.BuyerId);
        (await c.TrackingHandler.Handle(new(c.Orders.Parent.Id), default)).Value.Should().BeEmpty();
    }
}
