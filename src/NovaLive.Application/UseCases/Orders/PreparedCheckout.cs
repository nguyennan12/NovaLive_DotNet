using NovaLive.Domain.Carts;
using NovaLive.Domain.Users;
using NovaLive.Domain.Shops;
using NovaLive.Domain.Products;
using NovaLive.Domain.Discounts;
using NovaLive.Domain.Pricing;
using NovaLive.Domain.FlashSales;
namespace NovaLive.Application.UseCases.Orders;
public sealed record PreparedCheckout(Guid BuyerId, IReadOnlyList<CartItem> Items, UserAddress Address,
    IReadOnlyDictionary<Guid, ShopAddress> Warehouses, IReadOnlyDictionary<Guid, Shop> Shops,
    IReadOnlyDictionary<Guid, Sku> Skus, IReadOnlyDictionary<Guid, Spu> Spus,
    IReadOnlyList<VoucherSnapshot> Vouchers, PriceCalculationResult Price,
    IReadOnlyDictionary<Guid, FlashSaleItem> FlashSaleItems);
