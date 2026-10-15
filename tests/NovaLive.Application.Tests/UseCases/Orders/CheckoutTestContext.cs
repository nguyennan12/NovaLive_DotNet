using FluentAssertions;
using Moq;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Abstractions.ThirdParty;
using NovaLive.Application.UseCases.Orders.Queries.CalculateCheckoutDraft;
using NovaLive.Contracts.V1.Orders;
using NovaLive.Domain.Carts;
using NovaLive.Domain.Common;
using NovaLive.Domain.Discounts;
using NovaLive.Domain.Products;
using NovaLive.Domain.Shops;
using NovaLive.Domain.Users;
using Stock = NovaLive.Domain.Inventory.Inventory;

namespace NovaLive.Application.Tests.UseCases.Orders;

public sealed class CheckoutTestContext : IDisposable
{
    public Guid UserId { get; } = Guid.NewGuid();
    public DateTimeOffset Now { get; } = new(2026, 10, 15, 0, 0, 0, TimeSpan.Zero);
    public Mock<ICartRepository> Carts { get; } = new(MockBehavior.Strict);
    public Mock<ISkuRepository> Skus { get; } = new(MockBehavior.Strict);
    public Mock<ISpuRepository> Spus { get; } = new(MockBehavior.Strict);
    public Mock<IShopRepository> Shops { get; } = new(MockBehavior.Strict);
    public Mock<IInventoryRepository> Inventories { get; } = new(MockBehavior.Strict);
    public Mock<IUserAddressRepository> Addresses { get; } = new(MockBehavior.Strict);
    public Mock<IShopAddressRepository> Warehouses { get; } = new(MockBehavior.Strict);
    public Mock<IDiscountRepository> Discounts { get; } = new(MockBehavior.Strict);
    public Mock<IShippingProvider> Shipping { get; } = new(MockBehavior.Strict);
    public Mock<ICurrentUser> User { get; } = new(MockBehavior.Strict);
    public Mock<IDateTimeProvider> Clock { get; } = new(MockBehavior.Strict);
    public Cart Cart { get; }
    public UserAddress Address { get; }
    public List<CartItem> Items { get; } = [];
    public List<Sku> SkuList { get; } = [];
    public List<Spu> SpuList { get; } = [];
    public List<Shop> ShopList { get; } = [];
    public List<ShopAddress> WarehouseList { get; } = [];
    public Dictionary<Guid, Stock> Stocks { get; } = [];
    public List<Discount> VoucherList { get; } = [];

    public CheckoutTestContext()
    {
        Cart = new Cart(UserId, Now.AddDays(-1));
        Address = new UserAddress(UserId, "Buyer", "0900000000", 1, "Province", 200, "District", "W200", "Ward", "Street");
        User.SetupGet(x => x.IsAuthenticated).Returns(true);
        User.SetupGet(x => x.UserId).Returns(UserId);
        Clock.SetupGet(x => x.UtcNow).Returns(Now);
        Carts.Setup(x => x.GetByUserIdAsync(UserId, It.IsAny<CancellationToken>())).ReturnsAsync(Cart);
        Carts.Setup(x => x.GetCartItemsByIdsAsync(It.IsAny<IEnumerable<Guid>>(), UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<Guid> ids, Guid? _, CancellationToken _) => Items.Where(i => ids.Contains(i.Id)).ToList());
        Addresses.Setup(x => x.GetByIdForUserAsync(Address.Id, UserId, It.IsAny<CancellationToken>())).ReturnsAsync(Address);
        Skus.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<Guid> ids, CancellationToken _) => SkuList.Where(s => ids.Contains(s.Id)).ToList());
        Spus.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<Guid> ids, CancellationToken _) => SpuList.Where(s => ids.Contains(s.Id)).ToList());
        Shops.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<Guid> ids, CancellationToken _) => ShopList.Where(s => ids.Contains(s.Id)).ToList());
        Inventories.Setup(x => x.GetBySkuIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<Guid> ids, CancellationToken _) => Stocks.Where(s => ids.Contains(s.Key)).ToDictionary());
        Warehouses.Setup(x => x.GetPrimaryByShopIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<Guid> ids, CancellationToken _) => WarehouseList.Where(w => ids.Contains(w.ShopId)).ToList());
        Discounts.Setup(x => x.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string code, CancellationToken _) => VoucherList.SingleOrDefault(v => v.Code == code));
        Discounts.Setup(x => x.GetUserUsageCountAsync(It.IsAny<Guid>(), UserId, It.IsAny<CancellationToken>())).ReturnsAsync(0);
        Shipping.Setup(x => x.CalculateFeeAsync(It.IsAny<ShippingFeeCalculationModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new ShippingFeeQuote("GHN", "2", 30m)));
        AddShop();
    }

    public void AddShop(decimal price = 100m, int quantity = 2)
    {
        var shop = new Shop(Guid.NewGuid(), $"Shop {ShopList.Count}", Guid.NewGuid().ToString()); shop.Approve();
        var spu = new Spu(shop.Id, Guid.NewGuid(), "Product", null, null, "thumbnail", null);
        var sku = new Sku(spu.Id, shop.Id, "SKU", "{}", price, price);
        var warehouse = new ShopAddress(shop.Id, "Primary", "Seller", "0900000000", 1, "P",
            100 + ShopList.Count, "D", $"W{ShopList.Count}", "W", "Street"); warehouse.SetPrimary(true);
        ShopList.Add(shop); SpuList.Add(spu); SkuList.Add(sku); WarehouseList.Add(warehouse);
        Stocks.Add(sku.Id, new Stock(sku.Id, shop.Id, 10));
        Items.Add(new CartItem(Guid.NewGuid(), Cart.Id, sku.Id, shop.Id, quantity, 1m, Now.AddDays(-1)));
    }

    public Discount AddVoucher(string code, Guid? shopId = null, DiscountType type = DiscountType.FixedCart,
        decimal value = 10m, decimal minimum = 0m, bool active = true, int? maxUses = null,
        DiscountAppliesTo appliesTo = DiscountAppliesTo.AllProducts, List<Guid>? targets = null,
        DateTimeOffset? from = null, DateTimeOffset? to = null)
    {
        var voucher = new Discount(shopId, code, code, type, value, minimum, 1000m, maxUses, 1,
            appliesTo, targets, from ?? Now.AddDays(-1), to ?? Now.AddDays(1), true, active);
        VoucherList.Add(voucher);
        return voucher;
    }

    public CalculateCheckoutRequest Request => new(Items.Select(i => i.Id).ToList(), Address.Id, null, null, null);
    public CalculateCheckoutDraftQueryHandler Handler => new(Carts.Object, Skus.Object, Spus.Object, Shops.Object,
        Inventories.Object, Addresses.Object, Warehouses.Object, Discounts.Object, Shipping.Object, User.Object, Clock.Object);

    public Task<Result<CheckoutPreviewResponse>> Run(CalculateCheckoutRequest? request = null, CancellationToken ct = default) =>
        Handler.Handle(new CalculateCheckoutDraftQuery(request ?? Request), ct);

    public void Dispose()
    {
        // Strict mocks have no write setups: any write would already fail the test.
        Mock[] repositories = [Carts, Skus, Spus, Shops, Inventories, Addresses, Warehouses, Discounts];
        repositories.SelectMany(r => r.Invocations).Where(i => !i.Method.Name.StartsWith("Get")).Should().BeEmpty();
        Carts.Verify(x => x.AddAsync(It.IsAny<Cart>(), It.IsAny<CancellationToken>()), Times.Never);
        Carts.Verify(x => x.AddItemAsync(It.IsAny<CartItem>(), It.IsAny<CancellationToken>()), Times.Never);
        Carts.Verify(x => x.RemoveItems(It.IsAny<IEnumerable<CartItem>>()), Times.Never);
        Carts.Verify(x => x.TrySaveAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        Discounts.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
