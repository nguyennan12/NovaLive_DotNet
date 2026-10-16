namespace NovaLive.Contracts.V1.Orders;

public record ShopCheckoutPreviewDto(
    Guid ShopId,
    string ShopName,
    decimal ItemsSubtotal,
    decimal ShippingFee,
    decimal ShopDiscount,
    decimal AllocatedPlatformDiscount,
    decimal ShippingDiscount,
    decimal ShopTotal,
    List<CheckoutItemDto> Items);
