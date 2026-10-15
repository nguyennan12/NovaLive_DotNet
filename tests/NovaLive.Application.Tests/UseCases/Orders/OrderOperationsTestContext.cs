using Moq;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.UseCases.Orders;
using NovaLive.Application.UseCases.Shops;
using NovaLive.Application.UseCases.Orders.Commands.CancelOrder;
using NovaLive.Application.UseCases.Orders.Commands.CancelSubOrder;
using NovaLive.Application.UseCases.Orders.Commands.ConfirmSubOrder;
using NovaLive.Domain.Common;
using NovaLive.Domain.Orders;
namespace NovaLive.Application.Tests.UseCases.Orders;
public sealed class OrderOperationsTestContext
{
    public CheckoutCommandTestContext Checkout { get; } = new();
    public Mock<ICurrentUser> Seller { get; } = new();
    public Mock<ICurrentShop> Shop { get; } = new();
    public Guid SellerId { get; } = Guid.NewGuid();
    public int OwnedShopIndex { get; set; }
    public Guid HeaderShopId { get; set; }
    public ParentOrder Parent => Checkout.ParentList.Single();
    public OrderDetailsReader Reader => new(Checkout.Parents.Object, Checkout.Subs.Object,
        Checkout.Items.Object, Checkout.Histories.Object, Checkout.Payments.Object);
    public OrderReservationReleaser Releaser => new(Checkout.Items.Object, Checkout.Draft.Inventories.Object);
    public OrderShopAccess Access => new(new ShopAccess(Checkout.Draft.Shops.Object, Seller.Object), Shop.Object);
    public CancelOrderCommandHandler BuyerCancel => new(Checkout.Parents.Object, Checkout.Subs.Object, Checkout.Payments.Object,
        Checkout.Histories.Object, Checkout.Outbox.Object, Releaser, Checkout.Draft.User.Object, Checkout.Draft.Clock.Object);
    public CancelSubOrderCommandHandler SellerCancel => new(Checkout.Parents.Object, Checkout.Subs.Object, Checkout.Payments.Object,
        Checkout.Histories.Object, Checkout.Outbox.Object, Releaser, Seller.Object, Access, Checkout.Draft.Clock.Object);
    public ConfirmSubOrderCommandHandler Confirm => new(Checkout.Parents.Object, Checkout.Subs.Object, Checkout.Draft.Warehouses.Object,
        Checkout.Histories.Object, Seller.Object, Access, Checkout.Draft.Clock.Object);
    public static async Task<OrderOperationsTestContext> Create(bool cod = false, int shops = 1)
    {
        var c = new OrderOperationsTestContext();
        for (var i = 1; i < shops; i++) c.Checkout.Draft.AddShop();
        var result = await c.Checkout.Run(c.Checkout.Request with { PaymentMethod = cod ? "COD" : "MoMo" });
        if (result.IsFailure) throw new InvalidOperationException(result.Error.Code);
        c.HeaderShopId = c.Checkout.Draft.ShopList[0].Id;
        c.Seller.SetupGet(u => u.UserId).Returns(c.SellerId); c.Seller.SetupGet(u => u.IsAuthenticated).Returns(true);
        c.Shop.SetupGet(s => s.ShopId).Returns(() => c.HeaderShopId);
        c.Checkout.Draft.Shops.Setup(s => s.GetByOwnerIdAsync(c.SellerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => c.Checkout.Draft.ShopList[c.OwnedShopIndex]);
        c.Checkout.Parents.Setup(p => p.GetForUpdateAsync(c.Parent.Id, It.IsAny<CancellationToken>())).ReturnsAsync(c.Parent);
        c.Checkout.Parents.Setup(p => p.FindAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, Guid? buyer, CancellationToken _) => id == c.Parent.Id && (buyer is null || buyer == c.Parent.BuyerId) ? c.Parent : null);
        c.Checkout.Subs.Setup(s => s.GetByParentIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<Guid> ids, bool _, CancellationToken _) => c.Checkout.SubList.Where(s => ids.Contains(s.ParentOrderId)).ToList());
        c.Checkout.Subs.Setup(s => s.FindForShopAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, Guid shop, CancellationToken _) => c.Checkout.SubList.SingleOrDefault(s => s.Id == id && s.ShopId == shop));
        c.Checkout.Items.Setup(i => i.GetBySubOrderIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<Guid> ids, CancellationToken _) => c.Checkout.ItemList.Where(i => ids.Contains(i.SubOrderId)).ToList());
        c.Checkout.Histories.Setup(i => i.GetBySubOrderIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<Guid> ids, CancellationToken _) => c.Checkout.HistoryList.Where(i => ids.Contains(i.SubOrderId)).ToList());
        c.Checkout.Payments.Setup(p => p.GetForUpdateAsync(c.Parent.Id, It.IsAny<CancellationToken>())).ReturnsAsync(c.Checkout.PaymentList.Single());
        c.Checkout.Payments.Setup(p => p.GetByParentIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<Guid> ids, CancellationToken _) => c.Checkout.PaymentList.Where(p => ids.Contains(p.ParentOrderId)).ToList());
        c.Checkout.Draft.Warehouses.Setup(w => w.FindForShopAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, Guid shop, CancellationToken _) => c.Checkout.Draft.WarehouseList.SingleOrDefault(w => w.Id == id && w.ShopId == shop));
        return c;
    }
}
