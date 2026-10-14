using FluentAssertions;
using Moq;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.UseCases.Carts;
using NovaLive.Application.UseCases.Carts.Commands.AddToCart;
using NovaLive.Application.UseCases.Carts.Commands.UpdateCartItem;
using NovaLive.Application.UseCases.Carts.Commands.RemoveCartItem;
using NovaLive.Application.UseCases.Carts.Commands.RemoveSelectedCartItems;
using NovaLive.Application.UseCases.Carts.Queries.GetCart;
using NovaLive.Contracts.V1.Carts;
using NovaLive.Domain.Carts;
using NovaLive.Domain.Common;
using NovaLive.Domain.Products;
using NovaLive.Domain.Shops;
using StockEntity = NovaLive.Domain.Inventory.Inventory;
namespace NovaLive.Application.Tests.UseCases.Carts;
public sealed class CartTestContext
{
    public readonly Guid UserId = Guid.NewGuid();
    public readonly DateTimeOffset Now = new(2026, 10, 14, 0, 0, 0, TimeSpan.Zero);
    public Mock<ICartRepository> Carts { get; } = new();
    public Mock<ISkuRepository> Skus { get; } = new();
    public Mock<ISpuRepository> Spus { get; } = new();
    public Mock<IShopRepository> Shops { get; } = new();
    public Mock<IInventoryRepository> Inventories { get; } = new();
    public Mock<ICurrentUser> User { get; } = new();
    public Mock<IDateTimeProvider> Clock { get; } = new();
    public Cart? Cart { get; set; }
    public List<CartItem> Items { get; } = [];
    public Shop Shop { get; }
    public Spu Spu { get; }
    public Sku Sku { get; }
    public StockEntity Stock { get; }
    public CartTestContext()
    {
        Shop = new Shop(Guid.NewGuid(), "Shop", "shop"); Shop.Approve();
        Spu = new Spu(Shop.Id, Guid.NewGuid(), "Product", null, null, "thumbnail", null);
        Sku = new Sku(Spu.Id, Shop.Id, "SKU", null, 120m, 100m);
        Stock = new StockEntity(Sku.Id, Shop.Id, 10); Stock.Reserve(3);
        User.SetupGet(u => u.IsAuthenticated).Returns(true); User.SetupGet(u => u.UserId).Returns(UserId);
        Clock.SetupGet(c => c.UtcNow).Returns(Now);
        Carts.Setup(c => c.GetByUserIdAsync(UserId, It.IsAny<CancellationToken>())).ReturnsAsync(() => Cart);
        Carts.Setup(c => c.GetItemsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => Items.Where(i => i.CartId == id).ToList());
        Carts.Setup(c => c.AddAsync(It.IsAny<Cart>(), It.IsAny<CancellationToken>()))
            .Callback((Cart cart, CancellationToken _) => Cart = cart).Returns(Task.CompletedTask);
        Carts.Setup(c => c.AddItemAsync(It.IsAny<CartItem>(), It.IsAny<CancellationToken>()))
            .Callback((CartItem item, CancellationToken _) => Items.Add(item)).Returns(Task.CompletedTask);
        Carts.Setup(c => c.RemoveItems(It.IsAny<IEnumerable<CartItem>>()))
            .Callback((IEnumerable<CartItem> items) => { foreach (var item in items.ToArray()) Items.Remove(item); });
        Carts.Setup(c => c.TrySaveAsync(UserId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        Skus.Setup(s => s.GetByIdAsync(Sku.Id, It.IsAny<CancellationToken>())).ReturnsAsync(Sku);
        Skus.Setup(s => s.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync([Sku]);
        Spus.Setup(s => s.GetByIdAsync(Spu.Id, It.IsAny<CancellationToken>())).ReturnsAsync(Spu);
        Spus.Setup(s => s.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync([Spu]);
        Shops.Setup(s => s.GetActiveByIdAsync(Shop.Id, It.IsAny<CancellationToken>())).ReturnsAsync(Shop);
        Shops.Setup(s => s.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync([Shop]);
        Inventories.Setup(i => i.GetBySkuIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, StockEntity> { [Sku.Id] = Stock });
    }
    public CartItem OwnItem(int quantity = 2, decimal snapshot = 50m)
    {
        Cart ??= new Cart(UserId, Now.AddDays(-1));
        var item = Cart.AddItem(Items, Sku.Id, Shop.Id, quantity, snapshot, Now.AddDays(-1)).Value!;
        if (!Items.Contains(item)) Items.Add(item);
        return item;
    }
    public AddToCartCommandHandler AddHandler() => new(Carts.Object, Skus.Object, Spus.Object, Shops.Object, Inventories.Object, User.Object, Clock.Object);
    public UpdateCartItemCommandHandler UpdateHandler() => new(Carts.Object, Skus.Object, Inventories.Object, User.Object, Clock.Object);
    public RemoveCartItemCommandHandler RemoveHandler() => new(Carts.Object, User.Object, Clock.Object);
    public RemoveSelectedCartItemsCommandHandler RemoveSelectedHandler() => new(Carts.Object, User.Object, Clock.Object);
    public GetCartQueryHandler GetHandler() => new(Carts.Object, Skus.Object, Spus.Object, Shops.Object, Inventories.Object, User.Object);
}
