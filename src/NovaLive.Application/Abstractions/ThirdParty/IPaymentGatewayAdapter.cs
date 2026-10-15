using NovaLive.Contracts.V1.Payments;

namespace NovaLive.Application.Abstractions.ThirdParty;

/// <summary>
/// Giao diện bộ điều hợp cổng thanh toán trực tuyến.
/// </summary>
public interface IPaymentGatewayAdapter
{
    /// <summary>
    /// Tên cổng thanh toán (MoMo, VietQR, ...).
    /// </summary>
    string GatewayName { get; }

    /// <summary>
    /// Tạo yêu cầu thanh toán và nhận URL/mã QR để khách hàng tiến hành thanh toán.
    /// </summary>
    Task<PaymentIntentResult> CreatePaymentIntentAsync(PaymentIntentRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Giao diện chuyên biệt cho cổng thanh toán MoMo.
/// </summary>
public interface IMomoPaymentAdapter : IPaymentGatewayAdapter
{
    /// <summary>
    /// Sinh chữ ký HMAC-SHA256 và gọi API MoMo để tạo link thanh toán QRCode / App redirect.
    /// </summary>
    Task<MomoCreatePaymentResponse> CreatePaymentUrlAsync(
        string orderId,
        long amount,
        string orderInfo,
        string? extraData = null,
        CancellationToken ct = default);

    /// <summary>
    /// Kiểm tra tính hợp lệ của chữ ký HMAC-SHA256 do MoMo gửi sang trong IPN Webhook.
    /// </summary>
    bool VerifySignature(MomoIpnRequest ipnRequest);
}

/// <summary>
/// Giao diện chuyên biệt cho cổng thanh toán VietQR chuyển khoản ngân hàng.
/// </summary>
public interface IVietQrPaymentAdapter : IPaymentGatewayAdapter
{
    /// <summary>
    /// Sinh chuỗi mã VietQR chuẩn NAPAS 247 theo cú pháp nội dung chuyển khoản NOVA {order_code}.
    /// </summary>
    VietQrResponse GenerateDynamicQr(string orderCode, decimal amount, string? description = null);

    /// <summary>
    /// Xác thực chữ ký webhook (nếu cổng webhook cung cấp secret key hoặc token).
    /// </summary>
    bool VerifyWebhookSignature(string rawPayload, string? signature);

    /// <summary>
    /// Trích xuất mã đơn hàng orderCode từ nội dung chuyển khoản ngân hàng.
    /// </summary>
    string? ExtractOrderCode(string content);
}
