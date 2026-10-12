using NovaLive.Contracts.V1.Products;
using NovaLive.Domain.Products;
using InventoryEntity = NovaLive.Domain.Inventory.Inventory;

namespace NovaLive.Application.UseCases.Products;

public static class ProductMappings
{
    public static SkuResponse ToResponse(
        this Sku sku,
        InventoryEntity? inv,
        IEnumerable<string>? imageUrls = null)
    {
        return new SkuResponse(
            Id: sku.Id,
            SpuId: sku.SpuId,
            SkuCode: sku.SkuCode,
            AttributesJson: sku.AttributesJson ?? "{}",
            OriginalPrice: sku.OriginalPrice,
            SellPrice: sku.SellPrice,
            WeightGram: sku.WeightGram,
            QtyOnHand: inv?.QtyOnHand ?? 0,
            ReservedQty: inv?.ReservedQty ?? 0,
            AvailableQty: inv?.AvailableQty ?? 0,
            IsActive: sku.IsActive,
            Images: (imageUrls ?? []).ToList());
    }

    public static PublicSkuResponse ToPublicResponse(
        this Sku sku,
        InventoryEntity? inv,
        IEnumerable<string>? imageUrls = null)
    {
        var available = inv?.AvailableQty ?? 0;
        return new PublicSkuResponse(
            Id: sku.Id,
            SpuId: sku.SpuId,
            SkuCode: sku.SkuCode,
            AttributesJson: sku.AttributesJson ?? "{}",
            OriginalPrice: sku.OriginalPrice,
            SellPrice: sku.SellPrice,
            WeightGram: sku.WeightGram,
            InStock: available > 0,
            AvailableQty: available,
            Images: (imageUrls ?? []).ToList());
    }

    public static SpuDetailResponse ToDetailResponse(
        this Spu spu,
        string shopName,
        string categoryName,
        IEnumerable<SkuResponse> skus,
        IEnumerable<ProductAttributeDto> attributes)
    {
        var skuList = skus.ToList();
        var minPrice = skuList.Count > 0 ? skuList.Min(s => s.SellPrice) : 0m;
        var maxPrice = skuList.Count > 0 ? skuList.Max(s => s.SellPrice) : 0m;

        return new SpuDetailResponse(
            Id: spu.Id,
            ShopId: spu.ShopId,
            ShopName: shopName,
            CategoryId: spu.CategoryId,
            CategoryName: categoryName,
            Name: spu.Name,
            Slug: NovaLive.Application.Common.Helpers.SlugHelper.GenerateSlug(spu.Name),
            Description: spu.Description ?? string.Empty,
            Brand: spu.Brand,
            ThumbnailUrl: spu.ThumbnailUrl ?? string.Empty,
            AttributesConfigJson: spu.AttributesConfigJson,
            MinPrice: minPrice,
            MaxPrice: maxPrice,
            Rating: 5.0,
            SoldCount: 0,
            Status: spu.Status.ToString(),
            CreatedAt: spu.CreatedAt.UtcDateTime,
            Skus: skuList,
            Attributes: attributes.ToList());
    }

    public static PublicSpuDetailResponse ToPublicDetailResponse(
        this Spu spu,
        string shopName,
        string categoryName,
        IEnumerable<PublicSkuResponse> skus,
        IEnumerable<ProductAttributeDto> attributes)
    {
        var skuList = skus.ToList();
        var minPrice = skuList.Count > 0 ? skuList.Min(s => s.SellPrice) : 0m;
        var maxPrice = skuList.Count > 0 ? skuList.Max(s => s.SellPrice) : 0m;

        return new PublicSpuDetailResponse(
            Id: spu.Id,
            ShopId: spu.ShopId,
            ShopName: shopName,
            CategoryId: spu.CategoryId,
            CategoryName: categoryName,
            Name: spu.Name,
            Slug: NovaLive.Application.Common.Helpers.SlugHelper.GenerateSlug(spu.Name),
            Description: spu.Description ?? string.Empty,
            Brand: spu.Brand,
            ThumbnailUrl: spu.ThumbnailUrl ?? string.Empty,
            AttributesConfigJson: spu.AttributesConfigJson,
            MinPrice: minPrice,
            MaxPrice: maxPrice,
            Rating: 5.0,
            SoldCount: 0,
            Status: spu.Status.ToString(),
            CreatedAt: spu.CreatedAt.UtcDateTime,
            Skus: skuList,
            Attributes: attributes.ToList());
    }

    public static SpuResponse ToSummaryResponse(
        this Spu spu,
        string shopName,
        string categoryName,
        IEnumerable<Sku>? skus = null)
    {
        var skuList = (skus ?? []).ToList();
        var minPrice = skuList.Count > 0 ? skuList.Min(s => s.SellPrice) : 0m;
        var maxPrice = skuList.Count > 0 ? skuList.Max(s => s.SellPrice) : 0m;

        return new SpuResponse(
            Id: spu.Id,
            ShopId: spu.ShopId,
            ShopName: shopName,
            CategoryId: spu.CategoryId,
            CategoryName: categoryName,
            Name: spu.Name,
            Slug: NovaLive.Application.Common.Helpers.SlugHelper.GenerateSlug(spu.Name),
            Description: spu.Description ?? string.Empty,
            Brand: spu.Brand,
            ThumbnailUrl: spu.ThumbnailUrl ?? string.Empty,
            MinPrice: minPrice,
            MaxPrice: maxPrice,
            Rating: 5.0,
            SoldCount: 0,
            Status: spu.Status.ToString(),
            CreatedAt: spu.CreatedAt.UtcDateTime);
    }
}
