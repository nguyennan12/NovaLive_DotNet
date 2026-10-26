using Moq;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.ThirdParty;
using NovaLive.Application.Tests.UseCases.Orders;
using NovaLive.Application.UseCases.Shipping.Commands.CreateShipmentOrder;
using NovaLive.Application.UseCases.Shipping.Queries.GetOrderTracking;
using NovaLive.Application.UseCases.Shipping.Queries.GetShippingLabel;
using NovaLive.Contracts.V1.Shipping;
using NovaLive.Domain.Common;
using NovaLive.Domain.Orders;
using NovaLive.Domain.Shipping;

namespace NovaLive.Application.Tests.Shipping;

public sealed class ShippingFulfillmentTestContext
{
    public OrderOperationsTestContext Orders { get; private set; } = null!;
    public Mock<IShippingOrderRepository> Shipments { get; } = new();
    public Mock<IShippingProviderResolver> Resolver { get; } = new();
    public Mock<IShippingProvider> Provider { get; } = new(MockBehavior.Strict);
    public List<ShippingOrder> Saved { get; } = [];
    public SubOrder Sub => Orders.Checkout.SubList.First(s => s.ShopId == Orders.HeaderShopId);
    public CreateShippingOrderRequest Request => new(Sub.Id, "GHN", "2", Orders.Checkout.Draft.WarehouseList.First().Id);
    public CreateShipmentOrderCommandHandler CreateHandler => new(Orders.Checkout.Parents.Object, Orders.Checkout.Subs.Object,
        Shipments.Object, Orders.Checkout.Draft.Warehouses.Object, Orders.Checkout.Items.Object, Orders.Checkout.Draft.Skus.Object,
        Orders.Checkout.Payments.Object, Orders.Checkout.Histories.Object, Orders.Checkout.Outbox.Object,
        Orders.Seller.Object, Orders.Shop.Object, Orders.Access, Resolver.Object, Orders.Checkout.Draft.Clock.Object);
    public GetShippingLabelQueryHandler LabelHandler => new(Orders.Checkout.Subs.Object, Shipments.Object,
        Orders.Access, Orders.Shop.Object, Resolver.Object);
    public GetOrderTrackingQueryHandler TrackingHandler => new(Orders.Checkout.Parents.Object, Orders.Checkout.Subs.Object,
        Shipments.Object, Orders.Seller.Object, Orders.Shop.Object, Orders.Access, Resolver.Object);
    public Task<Result<ShippingOrderResponse>> Create() => CreateHandler.Handle(new(Request), default);

    public static async Task<ShippingFulfillmentTestContext> Build(bool cod = true, int shops = 1)
    {
        var c = new ShippingFulfillmentTestContext { Orders = await OrderOperationsTestContext.Create(cod, shops) };
        c.Orders.Checkout.HistoryList.Clear(); c.Orders.Checkout.Messages.Clear();
        c.Orders.Checkout.Payments.Setup(p => p.GetByParentOrderIdAsync(c.Orders.Parent.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(c.Orders.Checkout.PaymentList.Single());
        c.Resolver.Setup(p => p.Resolve(It.IsAny<string>())).Returns(Result<IShippingProvider>.Success(c.Provider.Object));
        c.Provider.Setup(p => p.CreateShipmentAsync(It.IsAny<ShippingShipmentCreateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ShippingOrderCreateResult("PROVIDER-1", "TRACK-1", 32000m));
        c.Provider.Setup(p => p.GetShippingLabelAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("%PDF-1.7"u8.ToArray());
        c.Provider.Setup(p => p.GetTrackingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ShippingTrackingResult("delivering", [new("delivering", c.Orders.Checkout.Draft.Now)]));
        c.Shipments.Setup(s => s.FindBySubOrderIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => c.Saved.SingleOrDefault(s => s.SubOrderId == id));
        c.Shipments.Setup(s => s.GetBySubOrderIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<Guid> ids, CancellationToken _) => c.Saved.Where(s => ids.Contains(s.SubOrderId)).ToList());
        c.Shipments.Setup(s => s.AddAsync(It.IsAny<ShippingOrder>(), It.IsAny<CancellationToken>()))
            .Callback((ShippingOrder s, CancellationToken _) => c.Saved.Add(s)).Returns(Task.CompletedTask);
        return c;
    }
}
