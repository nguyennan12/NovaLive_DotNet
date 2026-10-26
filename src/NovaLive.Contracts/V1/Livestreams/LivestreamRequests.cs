namespace NovaLive.Contracts.V1.Livestreams;

public record StartLivestreamRequest(
    string Title,
    string? ThumbnailUrl,
    Guid? WarehouseAddressId);

public sealed record ScheduleLivestreamRequest(
    string Title,
    string? ThumbnailUrl,
    DateTimeOffset ScheduledAt);

public sealed record UpdateScheduledLivestreamRequest(
    string Title,
    string? ThumbnailUrl,
    DateTimeOffset ScheduledAt);

public record PinProductRequest(
    Guid SkuId,
    decimal? FlashPrice,
    int? QuantityLimit,
    int? DurationSeconds = 120);

public record JoinLiveSessionRequest(
    string? GuestId = null);

public record QuickBuyCheckoutRequest(
    Guid SessionId,
    Guid SkuId,
    int Quantity,
    Guid ShippingAddressId,
    string PaymentMethod);

public record SendLivestreamCommentRequest(
    string Content,
    bool IsQuestion = false);

public record SendLivestreamReactionRequest(
    string Type);

