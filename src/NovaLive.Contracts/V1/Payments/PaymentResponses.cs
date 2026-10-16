using System.Text.Json.Serialization;

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

public record MomoCreatePaymentResponse
{
    [JsonPropertyName("partnerCode")]
    public string PartnerCode { get; init; } = string.Empty;

    [JsonPropertyName("orderId")]
    public string OrderId { get; init; } = string.Empty;

    [JsonPropertyName("requestId")]
    public string RequestId { get; init; } = string.Empty;

    [JsonPropertyName("amount")]
    public long Amount { get; init; }

    [JsonPropertyName("responseTime")]
    public long ResponseTime { get; init; }

    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;

    [JsonPropertyName("resultCode")]
    public int ResultCode { get; init; }

    [JsonPropertyName("payUrl")]
    public string? PayUrl { get; init; }

    [JsonPropertyName("deeplink")]
    public string? Deeplink { get; init; }

    [JsonPropertyName("qrCodeUrl")]
    public string? QrCodeUrl { get; init; }

    [JsonPropertyName("deeplinkMiniApp")]
    public string? DeeplinkMiniApp { get; init; }
}

public record MomoIpnResponse(
    [property: JsonPropertyName("resultCode")] int ResultCode,
    [property: JsonPropertyName("message")] string Message);

public record VietQrResponse(
    string QrCode,
    string QrDataUrl,
    string AccountName,
    string AccountNumber,
    string BankBin,
    decimal Amount,
    string Description);

public record VietQrWebhookResponse(
    bool Success,
    string Message,
    string? OrderCode = null);

public record PaymentSuccessEvent(
    Guid PaymentId,
    Guid ParentOrderId,
    string OrderCode,
    decimal Amount,
    string PaymentMethod,
    string? TransactionRef,
    DateTimeOffset PaidAt);
