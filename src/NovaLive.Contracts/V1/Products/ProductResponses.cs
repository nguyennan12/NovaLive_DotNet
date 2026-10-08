namespace NovaLive.Contracts.V1.Products;

public record SpuResponse(
    Guid Id,
    Guid ShopId,
    string ShopName,
    Guid CategoryId,
    string CategoryName,
    string Name,
    string Slug,
    string Description,
    string? Brand,
    string ThumbnailUrl,
    decimal MinPrice,
    decimal MaxPrice,
    double Rating,
    int SoldCount,
    string Status,
    DateTime CreatedAt);

public record SpuDetailResponse(
    Guid Id,
    Guid ShopId,
    string ShopName,
    Guid CategoryId,
    string CategoryName,
    string Name,
    string Slug,
    string Description,
    string? Brand,
    string ThumbnailUrl,
    decimal MinPrice,
    decimal MaxPrice,
    double Rating,
    int SoldCount,
    string Status,
    DateTime CreatedAt,
    List<SkuResponse> Skus,
    List<ProductAttributeDto> Attributes);

public record SkuResponse(
    Guid Id,
    Guid SpuId,
    string SkuCode,
    string AttributesJson,
    decimal OriginalPrice,
    decimal SellPrice,
    int WeightGram,
    int QtyOnHand,
    int ReservedQty,
    int AvailableQty,
    bool IsActive,
    List<string> Images);
