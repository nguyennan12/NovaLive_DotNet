using System.Text.Json.Serialization;

namespace NovaLive.Contracts.V1.Payments;

/// <summary>
/// Yêu cầu khởi tạo giao dịch thanh toán trực tuyến.
/// </summary>
public record InitiatePaymentRequest(
    Guid OrderId,
    string PaymentMethod);

/// <summary>
/// DTO chứa dữ liệu IPN gửi từ cổng thanh toán MoMo.
/// </summary>
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

/// <summary>
/// Yêu cầu tạo link thanh toán MoMo cho đơn hàng từ client (số tiền sẽ được hệ thống tự tính toán dựa trên đơn hàng).
/// </summary>
/// <param name="OrderId">Mã đơn hàng (OrderCode) hoặc ParentOrderId của khách hàng.</param>
/// <param name="OrderInfo">Ghi chú hoặc thông tin đơn hàng tuỳ chọn.</param>
/// <param name="ExtraData">Dữ liệu mở rộng MoMo (tuỳ chọn).</param>
public record CreateMomoPaymentUrlRequest(
    string OrderId,
    string? OrderInfo = null,
    string? ExtraData = null);

/// <summary>
/// DTO gửi sang MoMo API để tạo giao dịch thanh toán.
/// </summary>
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

/// <summary>
/// Payload webhook đối soát biến động số dư VietQR / SePAY / Casso / Ngân hàng.
/// </summary>
public record VietQrWebhookRequest
{
    [JsonPropertyName("transactionId")]
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

    [JsonPropertyName("signature")]
    public string? Signature { get; init; }

    [JsonPropertyName("transactionDate")]
    public string? TransactionDate { get; init; }

    /// <summary>
    /// Lấy ID giao dịch hợp nhất từ nhà cung cấp / ngân hàng. Trả về null nếu không có mã định danh nào.
    /// </summary>
    [JsonIgnore]
    public string? ResolvedTransactionId =>
        !string.IsNullOrWhiteSpace(TransactionId)
            ? TransactionId
            : Id?.ToString() ?? (!string.IsNullOrWhiteSpace(ReferenceCode) ? ReferenceCode : null);

    /// <summary>
    /// Lấy nội dung chuyển khoản hợp nhất.
    /// </summary>
    [JsonIgnore]
    public string ResolvedContent =>
        !string.IsNullOrWhiteSpace(Content)
            ? Content
            : Description ?? string.Empty;
}

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
