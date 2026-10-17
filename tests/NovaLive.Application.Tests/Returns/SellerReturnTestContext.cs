using Moq;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.UseCases.Orders;
using NovaLive.Application.UseCases.Shops;
using NovaLive.Application.UseCases.Returns;
using NovaLive.Application.UseCases.Returns.Commands.ApproveReturn;
using NovaLive.Application.UseCases.Returns.Commands.RejectReturn;
using NovaLive.Application.UseCases.Returns.Commands.ConfirmReturnReceived;
using NovaLive.Application.UseCases.Returns.Queries.GetSellerReturns;
using NovaLive.Domain.Common;
using NovaLive.Domain.Orders;
using NovaLive.Domain.Shops;
using NovaLive.Domain.Inventory;
namespace NovaLive.Application.Tests.Returns;
public sealed class SellerReturnTestContext
{
    public ReturnTestContext Buyer { get; } = new();
    public Guid SellerId { get; } = Guid.NewGuid();
    public Mock<ICurrentShop> CurrentShop { get; } = new();
    public Mock<IShopRepository> Shops { get; } = new();
    public Mock<IShopAddressRepository> Addresses { get; } = new();
    public Mock<IShopWalletRepository> Wallets { get; } = new();
    public Mock<IShopWalletTransactionRepository> WalletHistory { get; } = new();
    public Mock<IInventoryRepository> Inventory { get; } = new();
    public Mock<IInventoryHistoryRepository> StockHistory { get; } = new();
    public List<ShopWalletTransaction> WalletLedger { get; } = [];
    public List<InventoryHistory> StockLedger { get; } = [];
    public Shop Shop { get; }
    public ShopWallet Wallet { get; }
    public NovaLive.Domain.Inventory.Inventory Stock { get; }
    public ShopAddress Address { get; }
    public OrderReturn Request => Buyer.Saved.Last();
    public OrderShopAccess ShopAccess => new(new ShopAccess(Shops.Object, Buyer.User.Object), CurrentShop.Object);
    public SellerReturnAccess Access => new(Buyer.Returns.Object, Buyer.Subs.Object, ShopAccess, Buyer.User.Object);
    public ApproveReturnCommandHandler Approve => new(Access, Addresses.Object, Buyer.Reader, Buyer.Clock.Object);
    public RejectReturnCommandHandler Reject => new(Access, Buyer.Reader, Buyer.Clock.Object);
    public ConfirmReturnReceivedCommandHandler Received => new(Access, Buyer.Lines.Object, Buyer.Items.Object, Wallets.Object,
        WalletHistory.Object, Inventory.Object, StockHistory.Object, Buyer.Histories.Object, Buyer.Reader, Buyer.Clock.Object);
    public GetSellerReturnsQueryHandler List => new(ShopAccess, Buyer.Subs.Object, Buyer.Returns.Object, Buyer.Reader);
    public SellerReturnTestContext()
    {
        Shop = new Shop(SellerId, "Shop", "shop"); Shop.Approve();
        typeof(SubOrder).GetProperty(nameof(SubOrder.ShopId))!.SetValue(Buyer.Sub, Shop.Id);
        Wallet = new ShopWallet(Shop.Id); Wallet.Credit(1000, Buyer.Now);
        Stock = new(Buyer.Item.SkuId, Shop.Id, 5);
        Address = new ShopAddress(Shop.Id, "Return warehouse", "Seller", "0", 1, "P", 1, "D", "W", "W", "Return street"); Address.SetReturn(true);
        CurrentShop.SetupGet(s => s.ShopId).Returns(Shop.Id);
        Shops.Setup(s => s.GetByOwnerIdAsync(SellerId, It.IsAny<CancellationToken>())).ReturnsAsync(Shop);
        Buyer.Returns.Setup(r => r.GetForUpdateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => Buyer.Saved.SingleOrDefault(r => r.Id == id));
        Addresses.Setup(a => a.GetReturnAddressAsync(Shop.Id, It.IsAny<CancellationToken>())).ReturnsAsync(Address);
        Wallets.Setup(w => w.GetByShopIdForUpdateAsync(Shop.Id, It.IsAny<CancellationToken>())).ReturnsAsync(Wallet);
        Inventory.Setup(i => i.GetByIdsForUpdateAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync([Stock]);
        WalletHistory.Setup(h => h.AddAsync(It.IsAny<ShopWalletTransaction>(), It.IsAny<CancellationToken>()))
            .Callback((ShopWalletTransaction h, CancellationToken _) => WalletLedger.Add(h)).Returns(Task.CompletedTask);
        StockHistory.Setup(h => h.AddAsync(It.IsAny<InventoryHistory>(), It.IsAny<CancellationToken>()))
            .Callback((InventoryHistory h, CancellationToken _) => StockLedger.Add(h)).Returns(Task.CompletedTask);
        Buyer.Subs.Setup(s => s.GetIdsByShopAsync(Shop.Id, It.IsAny<CancellationToken>())).ReturnsAsync([Buyer.Sub.Id]);
        Buyer.Returns.Setup(r => r.GetSellerPagedAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<ReturnStatus?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<Guid> ids, ReturnStatus? state, int page, int size, CancellationToken _) =>
            { var rows = Buyer.Saved.Where(r => ids.Contains(r.SubOrderId) && (state == null || r.Status == state)).ToList(); return (rows.Skip((page - 1) * size).Take(size).ToList(), (long)rows.Count); });
    }
    public static async Task<SellerReturnTestContext> Create(bool approve = false)
    {
        var c = new SellerReturnTestContext();
        await c.Buyer.Handler.Handle(new(c.Buyer.Request), default);
        c.Buyer.User.SetupGet(u => u.UserId).Returns(c.SellerId);
        if (approve) await c.Approve.Handle(new(c.Request.Id, new("Accepted")), default);
        return c;
    }
}
