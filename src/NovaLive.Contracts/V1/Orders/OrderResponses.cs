namespace NovaLive.Contracts.V1.Orders;

public record CheckoutPreviewResponse(
    decimal TotalItemsAmount,
    decimal TotalShippingFee,
    decimal TotalShopDiscount,
    decimal TotalPlatformDiscount,
    decimal GrandTotal,
    List<ShopCheckoutPreviewDto> ShopPreviews);

public record ShopCheckoutPreviewDto(
    Guid ShopId,
    string ShopName,
    decimal ItemsSubtotal,
    decimal ShippingFee,
    decimal ShopDiscount,
    decimal AllocatedPlatformDiscount,
    decimal ShopTotal,
    List<CheckoutItemDto> Items);

public record CheckoutItemDto(
    Guid SkuId,
    string SpuName,
    string SkuCode,
    string AttributesJson,
    string ThumbnailUrl,
    decimal Price,
    int Quantity,
    decimal LineTotal);

public record OrderSubmitResponse(
    Guid ParentOrderId,
    string ParentOrderCode,
    decimal GrandTotal,
    string PaymentMethod,
    Guid? PaymentId,
    string? PaymentUrl,
    string? QrCodeUrl,
    List<Guid> SubOrderIds);

public record ParentOrderResponse(
    Guid Id,
    string OrderCode,
    Guid BuyerId,
    decimal GrandTotal,
    string PaymentMethod,
    string PaymentStatus,
    DateTime CreatedAt,
    List<SubOrderResponse> SubOrders);

public record SubOrderResponse(
    Guid Id,
    string SubOrderCode,
    Guid ShopId,
    string ShopName,
    decimal TotalAmount,
    decimal ShippingFee,
    decimal ShopDiscountAmount,
    decimal PlatformDiscountAmount,
    decimal FinalAmount,
    string Status,
    string? TrackingCode,
    DateTime CreatedAt,
    List<OrderItemResponse> Items);

public record OrderItemResponse(
    Guid Id,
    Guid SkuId,
    string SpuName,
    string SkuCode,
    string AttributesJson,
    string ThumbnailUrl,
    decimal Price,
    int Quantity,
    decimal LineTotal);
