namespace NovaLive.Contracts.V1.Livestreams;

public record StartLivestreamRequest(
    string Title,
    string? Description,
    string? BannerUrl,
    Guid WarehouseAddressId);

public record PinLivestreamProductRequest(
    Guid SkuId,
    decimal? FlashPrice,
    int? QuantityLimit);

public record QuickBuyCheckoutRequest(
    Guid SessionId,
    Guid SkuId,
    int Quantity,
    Guid ShippingAddressId,
    string PaymentMethod);
