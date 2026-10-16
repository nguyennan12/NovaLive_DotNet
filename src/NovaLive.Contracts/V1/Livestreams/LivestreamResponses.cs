namespace NovaLive.Contracts.V1.Livestreams;

public record LivestreamSessionResponse(
    Guid Id,
    Guid ShopId,
    string ShopName,
    string? ShopLogoUrl,
    string Title,
    string? ThumbnailUrl,
    string ChannelName,
    string Status,
    int ViewerCount,
    int PeakViewerCount,
    DateTimeOffset? StartedAt,
    DateTimeOffset? EndedAt);

public record LivestreamStartResponse(
    Guid Id,
    Guid ShopId,
    string ShopName,
    string? ShopLogoUrl,
    string Title,
    string? ThumbnailUrl,
    string ChannelName,
    string PublisherToken,
    string Status,
    int ViewerCount,
    DateTimeOffset StartedAt);

public record LivestreamSessionDetailResponse(
    Guid Id,
    Guid ShopId,
    string ShopName,
    string? ShopLogoUrl,
    string Title,
    string? ThumbnailUrl,
    string ChannelName,
    string Status,
    int ViewerCount,
    int PeakViewerCount,
    DateTimeOffset? StartedAt,
    DateTimeOffset? EndedAt,
    PinnedProductResponse? ActivePinnedProduct);

public record LivestreamTokenResponse(
    Guid SessionId,
    string ChannelName,
    string Token,
    string Account,
    string Role,
    int ExpiresInSeconds);

public record PinnedProductResponse(
    Guid SessionId,
    Guid SkuId,
    Guid SpuId,
    string SpuName,
    string SkuCode,
    string? AttributesJson,
    string? ThumbnailUrl,
    decimal OriginalPrice,
    decimal SellPrice,
    decimal? FlashPrice,
    decimal EffectivePrice,
    int? QuantityLimit,
    int SoldInLive,
    bool IsPinned,
    DateTimeOffset? PinnedAt,
    DateTimeOffset? ExpiresAt,
    int AvailableQty);

public record EndLiveSessionSummaryResponse(
    Guid SessionId,
    string Title,
    TimeSpan Duration,
    int PeakViewerCount,
    int TotalProductsSold,
    decimal TotalEstimatedRevenue,
    DateTimeOffset StartedAt,
    DateTimeOffset EndedAt);
