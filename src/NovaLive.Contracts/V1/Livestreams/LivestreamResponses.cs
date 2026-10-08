namespace NovaLive.Contracts.V1.Livestreams;

public record LivestreamSessionResponse(
    Guid Id,
    Guid ShopId,
    string ShopName,
    string? ShopLogoUrl,
    string Title,
    string? Description,
    string? BannerUrl,
    string ChannelName,
    string AgoraRtcToken,
    string Status,
    int ViewerCount,
    DateTime StartedAt,
    DateTime? EndedAt);

public record PinnedProductDto(
    Guid SkuId,
    Guid SpuId,
    string SpuName,
    string SkuCode,
    string AttributesJson,
    string ThumbnailUrl,
    decimal OriginalPrice,
    decimal SpecialPrice,
    int AvailableQty);
