namespace NovaLive.Contracts.V1.Orders;
public sealed record ParentOrderListItemResponse(Guid Id, string OrderCode, decimal GrandTotal, string PaymentStatus, DateTimeOffset CreatedAt);
