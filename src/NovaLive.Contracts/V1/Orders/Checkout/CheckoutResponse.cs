namespace NovaLive.Contracts.V1.Orders;
public sealed record CheckoutResponse(Guid ParentOrderId, string OrderCode, decimal GrandTotal,
    string PaymentMethod, Guid PaymentId, string PaymentStatus,
    IReadOnlyList<CheckoutSubOrderResponse> SubOrders, DateTimeOffset? ExpiredAt);
