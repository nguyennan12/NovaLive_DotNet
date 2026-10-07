namespace NovaLive.Contracts.Products;

public sealed record ProductSummaryDto(
    Guid SpuId,
    Guid ShopId,
    string Name,
    string? ThumbnailUrl,
    decimal MinPrice,
    decimal MaxPrice,
    decimal SearchRank);
