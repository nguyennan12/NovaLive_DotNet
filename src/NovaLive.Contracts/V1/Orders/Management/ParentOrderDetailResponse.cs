namespace NovaLive.Contracts.V1.Orders;
public sealed record ParentOrderDetailResponse(Guid Id, string OrderCode, OrderRecipientResponse Recipient,
    decimal TotalItemAmount, decimal TotalShippingFee, decimal TotalDiscountAmount, decimal GrandTotal,
    string CurrencyCode, string PaymentStatus, string PaymentMethod, Guid? PaymentId, DateTimeOffset? ExpiredAt,
    string? Note, DateTimeOffset CreatedAt, IReadOnlyList<SubOrderDetailResponse> SubOrders);
