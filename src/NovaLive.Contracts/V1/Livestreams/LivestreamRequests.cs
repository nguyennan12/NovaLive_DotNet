namespace NovaLive.Contracts.V1.Livestreams;

public record StartLivestreamRequest(
    string Title,
    string? ThumbnailUrl,
    Guid? WarehouseAddressId);

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
