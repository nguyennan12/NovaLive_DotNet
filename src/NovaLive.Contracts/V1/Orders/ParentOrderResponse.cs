namespace NovaLive.Contracts.V1.Orders;

public record ParentOrderResponse(
    Guid Id,
    string OrderCode,
    Guid BuyerId,
    decimal GrandTotal,
    string PaymentMethod,
    string PaymentStatus,
    DateTime CreatedAt,
    List<SubOrderResponse> SubOrders);
