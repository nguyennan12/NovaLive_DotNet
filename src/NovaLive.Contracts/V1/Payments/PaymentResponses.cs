namespace NovaLive.Contracts.V1.Payments;

public record PaymentInitiateResponse(
    Guid PaymentId,
    string PaymentMethod,
    decimal Amount,
    string Status,
    string? PaymentUrl,
    string? QrCodeUrl,
    DateTime ExpiresAt);

public record PaymentStatusResponse(
    Guid PaymentId,
    Guid OrderId,
    string PaymentMethod,
    decimal Amount,
    string Status,
    DateTime? PaidAt);
