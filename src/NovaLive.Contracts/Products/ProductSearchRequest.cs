namespace NovaLive.Contracts.Products;

public sealed record ProductSearchRequest(
    string? Keyword,
    Guid? ShopId,
    Guid? CategoryId,
    decimal? MinPrice,
    decimal? MaxPrice,
    string? Sort,
    int Page = 1,
    int Size = 20);
