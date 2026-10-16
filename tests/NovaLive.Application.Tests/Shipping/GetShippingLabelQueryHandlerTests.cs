using FluentAssertions;
using Moq;
using NovaLive.Application.UseCases.Shipping;

namespace NovaLive.Application.Tests.Shipping;

public sealed class GetShippingLabelQueryHandlerTests
{
    [Fact]
    public async Task OwnedShipment_ReturnsPdf()
    {
        var c = await ShippingFulfillmentTestContext.Build(); await c.Create();
        var result = await c.LabelHandler.Handle(new(c.Sub.Id), default);
        result.IsSuccess.Should().BeTrue(); result.Value.Should().Equal("%PDF-1.7"u8.ToArray());
    }

    [Fact]
    public async Task MissingShipment_IsNotFound()
    {
        var c = await ShippingFulfillmentTestContext.Build();
        (await c.LabelHandler.Handle(new(c.Sub.Id), default)).Error.Should().Be(ShippingErrors.ShipmentNotFound);
    }

    [Fact]
    public async Task ForeignShop_IsHiddenBeforeReadingShipment()
    {
        var c = await ShippingFulfillmentTestContext.Build(shops: 2);
        var foreign = c.Orders.Checkout.SubList.Single(s => s.ShopId != c.Sub.ShopId);
        (await c.LabelHandler.Handle(new(foreign.Id), default)).Error.Should().Be(ShippingErrors.SubOrderNotFound);
        c.Shipments.Verify(s => s.FindBySubOrderIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProviderHtml_IsNotReturnedAsPdf()
    {
        var c = await ShippingFulfillmentTestContext.Build(); await c.Create();
        c.Provider.Setup(p => p.GetShippingLabelAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("<html>print page</html>"u8.ToArray());
        (await c.LabelHandler.Handle(new(c.Sub.Id), default)).Error.Code.Should().Be("Shipping.ProviderError");
    }
}
