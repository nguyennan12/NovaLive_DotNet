using NovaLive.Contracts.V1.Orders;
using NovaLive.Domain.Pricing;
using NovaLive.Domain.Products;
using NovaLive.Domain.Shops;

namespace NovaLive.Application.UseCases.Orders;

public static class CheckoutMappings
{
    public static CheckoutPreviewResponse ToPreview(this PriceCalculationResult result,
        IReadOnlyDictionary<Guid, Shop> shops, IReadOnlyDictionary<Guid, Sku> skus,
        IReadOnlyDictionary<Guid, Spu> spus) => new(
        result.TotalItemAmount, result.TotalShippingFee, result.TotalShopDiscount,
        result.TotalPlatformDiscount, result.TotalShippingDiscount, result.GrandTotal,
        result.ShopResults.Select(shop => new ShopCheckoutPreviewDto(
            shop.ShopId, shops[shop.ShopId].ShopName, shop.ItemsSubtotal, shop.ShippingFee,
            shop.ShopDiscountAmount, shop.PlatformDiscountAmount, shop.ShippingDiscountAmount, shop.ShopTotal,
            shop.Items.Select(item => new CheckoutItemDto(
                item.SkuId, spus[item.SpuId].Name, skus[item.SkuId].SkuCode,
                skus[item.SkuId].AttributesJson ?? "{}", spus[item.SpuId].ThumbnailUrl ?? string.Empty,
                item.UnitPrice, item.Quantity, item.FinalLineTotal)).ToList())).ToList());
}
