namespace NovaLive.Contracts.V1.Orders;

public record SubOrderResponse(
    Guid Id,
    string SubOrderCode,
    Guid ShopId,
    string ShopName,
    decimal TotalAmount,
    decimal ShippingFee,
    decimal ShopDiscountAmount,
    decimal PlatformDiscountAmount,
    decimal ShippingDiscountAmount,
    decimal FinalAmount,
    string Status,
    string? TrackingCode,
    DateTime CreatedAt,
    List<OrderItemResponse> Items);
