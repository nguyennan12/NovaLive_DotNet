namespace NovaLive.Contracts.V1.Orders;

public record OrderSubmitResponse(
    Guid ParentOrderId,
    string ParentOrderCode,
    decimal GrandTotal,
    string PaymentMethod,
    Guid? PaymentId,
    string? PaymentUrl,
    string? QrCodeUrl,
    List<Guid> SubOrderIds);
