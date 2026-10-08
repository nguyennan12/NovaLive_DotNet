namespace NovaLive.Contracts.V1.Payments;

public record InitiatePaymentRequest(
    Guid OrderId,
    string PaymentMethod);

public record MoMoWebhookPayload(
    string PartnerCode,
    string OrderId,
    string RequestId,
    long Amount,
    string OrderInfo,
    string OrderType,
    long TransId,
    int ResultCode,
    string Message,
    string PayType,
    long ResponseTime,
    string ExtraData,
    string Signature);

public record VietQRWebhookPayload(
    string TransactionId,
    string AccountNumber,
    decimal Amount,
    string Content,
    string BankBrand,
    string Signature);
