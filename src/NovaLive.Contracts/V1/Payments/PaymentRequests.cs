using System.Text.Json.Serialization;

namespace NovaLive.Contracts.V1.Payments;

public record InitiatePaymentRequest(
    Guid ParentOrderId,
    string PaymentMethod);

public record MomoIpnRequest
{
    [JsonPropertyName("partnerCode")]
    public string PartnerCode { get; init; } = string.Empty;

    [JsonPropertyName("orderId")]
    public string OrderId { get; init; } = string.Empty;

    [JsonPropertyName("requestId")]
    public string RequestId { get; init; } = string.Empty;

    [JsonPropertyName("amount")]
    public long Amount { get; init; }

    [JsonPropertyName("orderInfo")]
    public string OrderInfo { get; init; } = string.Empty;

    [JsonPropertyName("orderType")]
    public string OrderType { get; init; } = string.Empty;

    [JsonPropertyName("transId")]
    public long TransId { get; init; }

    [JsonPropertyName("resultCode")]
    public int ResultCode { get; init; }

    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;

    [JsonPropertyName("payType")]
    public string PayType { get; init; } = string.Empty;

    [JsonPropertyName("responseTime")]
    public long ResponseTime { get; init; }

    [JsonPropertyName("extraData")]
    public string ExtraData { get; init; } = string.Empty;

    [JsonPropertyName("signature")]
    public string Signature { get; init; } = string.Empty;
}

public record CreateMomoPaymentUrlRequest(
    string OrderId,
    string? OrderInfo = null,
    string? ExtraData = null);

public record MomoCreatePaymentRequest(
    string PartnerCode,
    string RequestId,
    long Amount,
    string OrderId,
    string OrderInfo,
    string RedirectUrl,
    string IpnUrl,
    string RequestType,
    string ExtraData,
    string Lang,
    string Signature);

public record VietQrWebhookRequest
{
    [JsonPropertyName("transactionid")]
    public string? TransactionId { get; init; }

    [JsonPropertyName("id")]
    public long? Id { get; init; }

    [JsonPropertyName("accountNumber")]
    public string? AccountNumber { get; init; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; init; }

    [JsonPropertyName("content")]
    public string? Content { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("bankBrand")]
    public string? BankBrand { get; init; }

    [JsonPropertyName("gateway")]
    public string? Gateway { get; init; }

    [JsonPropertyName("referenceCode")]
    public string? ReferenceCode { get; init; }

    [JsonPropertyName("referencenumber")]
    public string? ReferenceNumber { get; init; }

    [JsonPropertyName("orderId")]
    public string? OrderId { get; init; }

    [JsonPropertyName("bankaccount")]
    public string? BankAccount { get; init; }

    [JsonPropertyName("transType")]
    public string? TransType { get; init; }

    [JsonPropertyName("transactiontime")]
    public long? TransactionTime { get; init; }

    [JsonPropertyName("signature")]
    public string? Signature { get; init; }

    [JsonPropertyName("transactionDate")]
    public string? TransactionDate { get; init; }

    [JsonIgnore]
    public string? ResolvedTransactionId =>
        !string.IsNullOrWhiteSpace(TransactionId)
            ? TransactionId
            : Id?.ToString() ?? (!string.IsNullOrWhiteSpace(ReferenceNumber)
                ? ReferenceNumber
                : (!string.IsNullOrWhiteSpace(ReferenceCode) ? ReferenceCode : null));

    [JsonIgnore]
    public string ResolvedContent =>
        !string.IsNullOrWhiteSpace(Content)
            ? Content
            : Description ?? string.Empty;
}

public sealed record VietQrTokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("token_type")] string TokenType,
    [property: JsonPropertyName("expires_in")] int ExpiresIn);

public sealed record VietQrTransactionSyncObject(
    [property: JsonPropertyName("reftransactionid")] string RefTransactionId);

public sealed record VietQrTransactionSyncResponse(
    [property: JsonPropertyName("error")] bool Error,
    [property: JsonPropertyName("errorReason")] string? ErrorReason,
    [property: JsonPropertyName("toastMessage")] string ToastMessage,
    [property: JsonPropertyName("object")] VietQrTransactionSyncObject? Object);

public sealed record VietQrTestCallbackResponse(
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("message")] string? Message);

// Backward compatibility aliases
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
