namespace NovaLive.Infrastructure.Payments.VietQr;

/// <summary>
/// Các cấu hình kết nối và tạo mã VietQR theo chuẩn NAPAS 247.
/// </summary>
public sealed class VietQrOptions
{
    public const string SectionName = "VietQr";

    /// <summary>
    /// Mã BIN ngân hàng thụ hưởng (ví dụ 970422 - MBBank, 970436 - Vietcombank, 970415 - VietinBank).
    /// </summary>
    public string BankBin { get; set; } = "970422";

    /// <summary>
    /// Số tài khoản ngân hàng thụ hưởng.
    /// </summary>
    public string AccountNumber { get; set; } = "0000123456789";

    /// <summary>
    /// Tên chủ tài khoản ngân hàng thụ hưởng (không dấu, viết hoa).
    /// </summary>
    public string AccountName { get; set; } = "CONG TY NOVALIVE";

    /// <summary>
    /// Client ID hoặc Merchant ID cấp từ hệ thống VietQR (ví dụ: customer-nan-user26703).
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// API Key / Secret Key xác thực từ cổng VietQR.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Khóa bí mật dùng để xác thực webhook gửi đến từ đối tác (SePAY, Casso, VietQR,...).
    /// </summary>
    public string WebhookSecret { get; set; } = string.Empty;

    /// <summary>
    /// Địa chỉ cơ sở của API VietQR.
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.vietqr.vn";
}
