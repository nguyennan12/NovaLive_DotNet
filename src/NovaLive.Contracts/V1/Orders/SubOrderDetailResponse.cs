namespace NovaLive.Contracts.V1.Orders;
public sealed record SubOrderDetailResponse(Guid Id, Guid ParentOrderId, string Code, Guid ShopId,
    Guid? WarehouseAddressId, string OrderSource, OrderRecipientResponse Recipient,
    decimal ItemAmount, decimal ShippingFee, decimal ShopDiscountAmount, decimal PlatformDiscountAmount,
    decimal ShippingDiscountAmount, decimal SubTotal, string Status, string? CancelReason,
    string PaymentStatus, string PaymentMethod, DateTimeOffset CreatedAt,
    IReadOnlyList<OrderItemDetailResponse> Items, IReadOnlyList<OrderTimelineResponse> Timeline);
