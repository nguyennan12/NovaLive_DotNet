namespace NovaLive.Contracts.V1.Products;

public record CreateSpuRequest(
    string Name,
    string Description,
    Guid CategoryId,
    string? Brand,
    string ThumbnailUrl,
    List<CreateSkuDto> Skus,
    List<ProductAttributeDto>? Attributes);

public record UpdateSpuRequest(
    string Name,
    string Description,
    Guid CategoryId,
    string? Brand,
    string ThumbnailUrl,
    List<ProductAttributeDto>? Attributes);

public record CreateSkuDto(
    string SkuCode,
    string AttributesJson,
    decimal OriginalPrice,
    decimal SellPrice,
    int WeightGram,
    int InitialStock,
    List<string>? Images);

public record UpdateSkuPriceRequest(
    decimal SellPrice,
    decimal OriginalPrice,
    int WeightGram,
    bool IsActive);

public record ProductAttributeDto(
    string Name,
    string Value);

public record SearchProductsRequest(
    Guid? ShopId,
    Guid? CategoryId,
    string? Keyword,
    decimal? MinPrice,
    decimal? MaxPrice,
    string? SortBy,
    int Page = 1,
    int Size = 20);
