namespace NovaLive.Contracts.V1.Orders;

public record CheckoutPreviewResponse(
    decimal TotalItemsAmount,
    decimal TotalShippingFee,
    decimal TotalShopDiscount,
    decimal TotalPlatformDiscount,
    decimal TotalShippingDiscount,
    decimal GrandTotal,
    List<ShopCheckoutPreviewDto> ShopPreviews);
