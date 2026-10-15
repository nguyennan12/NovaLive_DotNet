using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NovaLive.Application.Abstractions.ThirdParty;
using NovaLive.Contracts.V1.Payments;

namespace NovaLive.Infrastructure.Payments.VietQr;

/// <summary>
/// Bộ điều hợp thanh toán VietQR: sinh mã QR động chuẩn NAPAS 247 và xử lý đối soát webhook.
/// </summary>
public sealed class VietQrPaymentAdapter : IVietQrPaymentAdapter
{
    private readonly VietQrOptions _options;
    private readonly ILogger<VietQrPaymentAdapter> _logger;

    private static readonly Regex OrderCodeRegex = new(
        @"(?:NOVA|VQR)[\s_-]*([A-Za-z0-9_-]+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public string GatewayName => "VietQR";

    public VietQrPaymentAdapter(
        IOptions<VietQrOptions> options,
        ILogger<VietQrPaymentAdapter> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Sinh chuỗi mã VietQR chuẩn NAPAS 247 theo cú pháp nội dung chuyển khoản NOVA {order_code}.
    /// </summary>
    public VietQrResponse GenerateDynamicQr(string orderCode, decimal amount, string? description = null)
    {
        var cleanOrderCode = orderCode.Trim();
        var content = string.IsNullOrWhiteSpace(description)
            ? $"NOVA {cleanOrderCode}"
            : description.Trim();

        var intAmount = (long)Math.Round(amount, MidpointRounding.AwayFromZero);

        // 1. Tạo chuỗi mã QR theo chuẩn EMVCo NAPAS 247
        var emvcoQr = BuildNapasEmvcoQr(_options.BankBin, _options.AccountNumber, intAmount, content);

        // 2. Tạo URL ảnh mã QR QuickLink
        var encodedContent = Uri.EscapeDataString(content);
        var encodedName = Uri.EscapeDataString(_options.AccountName);
        var qrDataUrl = $"https://img.vietqr.io/image/{_options.BankBin}-{_options.AccountNumber}-compact2.png?amount={intAmount}&addInfo={encodedContent}&accountName={encodedName}";

        _logger.LogInformation(
            "Đã sinh mã VietQR cho đơn hàng {OrderCode}, số tiền: {Amount} VND, nội dung: '{Content}'",
            cleanOrderCode, intAmount, content);

        return new VietQrResponse(
            QrCode: emvcoQr,
            QrDataUrl: qrDataUrl,
            AccountName: _options.AccountName,
            AccountNumber: _options.AccountNumber,
            BankBin: _options.BankBin,
            Amount: intAmount,
            Description: content);
    }

    /// <summary>
    /// Trích xuất mã đơn hàng orderCode từ nội dung chuyển khoản ngân hàng.
    /// Ví dụ: "NOVA ORD-12345" -> "ORD-12345", "VQR12345" -> "12345".
    /// </summary>
    public string? ExtractOrderCode(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        var match = OrderCodeRegex.Match(content);
        if (match.Success && match.Groups.Count > 1)
        {
            return match.Groups[1].Value.Trim();
        }

        // Trường hợp người dùng chuyển khoản ghi thẳng mã đơn không có tiền tố NOVA
        var trimmed = content.Trim();
        if (!trimmed.Contains(' ') && trimmed.Length >= 4)
        {
            return trimmed;
        }

        return null;
    }

    /// <summary>
    /// Xác thực chữ ký webhook (nếu đối tác gửi kèm chữ ký HMAC hoặc secret token).
    /// </summary>
    public bool VerifyWebhookSignature(string rawPayload, string? signature)
    {
        if (string.IsNullOrWhiteSpace(_options.WebhookSecret))
        {
            // Nếu không cấu hình WebhookSecret thì chấp nhận (dùng IP whitelist hoặc bảo mật qua nội dung đối soát)
            return true;
        }

        if (string.IsNullOrWhiteSpace(signature))
        {
            return false;
        }

        var keyBytes = Encoding.UTF8.GetBytes(_options.WebhookSecret);
        var messageBytes = Encoding.UTF8.GetBytes(rawPayload);

        using var hmac = new HMACSHA256(keyBytes);
        var computedHash = Convert.ToHexString(hmac.ComputeHash(messageBytes)).ToLowerInvariant();

        var expectedBytes = Encoding.UTF8.GetBytes(computedHash);
        var actualBytes = Encoding.UTF8.GetBytes(signature.ToLowerInvariant());

        if (expectedBytes.Length != actualBytes.Length)
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(expectedBytes, actualBytes);
    }

    /// <summary>
    /// Tạo PaymentIntent chuẩn IPaymentGatewayAdapter.
    /// </summary>
    public Task<PaymentIntentResult> CreatePaymentIntentAsync(
        PaymentIntentRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = GenerateDynamicQr(request.PaymentId.ToString("N"), request.Amount, request.Description);
        return Task.FromResult(new PaymentIntentResult(response.Description, response.QrDataUrl, response.QrCode));
    }

    #region EMVCo NAPAS 247 Generator Helpers

    private static string BuildNapasEmvcoQr(string bankBin, string accountNumber, long amount, string content)
    {
        var sb = new StringBuilder();

        // Tag 00: Payload Format Indicator (01)
        AppendTlv(sb, "00", "01");

        // Tag 01: Point of Initiation Method (12: Dynamic QR)
        AppendTlv(sb, "01", "12");

        // Tag 38: Merchant Account Information (NAPAS)
        var maiBuilder = new StringBuilder();
        // Subtag 00: GUID "A000000727"
        AppendTlv(maiBuilder, "00", "A000000727");

        // Subtag 01: Beneficiary Organization & Account
        var bnBuilder = new StringBuilder();
        AppendTlv(bnBuilder, "00", bankBin);
        AppendTlv(bnBuilder, "01", accountNumber);
        AppendTlv(maiBuilder, "01", bnBuilder.ToString());

        // Subtag 02: Service Code "QRIBFTTA"
        AppendTlv(maiBuilder, "02", "QRIBFTTA");
        AppendTlv(sb, "38", maiBuilder.ToString());

        // Tag 53: Transaction Currency (704: VND)
        AppendTlv(sb, "53", "704");

        // Tag 54: Transaction Amount
        if (amount > 0)
        {
            AppendTlv(sb, "54", amount.ToString());
        }

        // Tag 58: Country Code (VN)
        AppendTlv(sb, "58", "VN");

        // Tag 62: Additional Data Field Template
        if (!string.IsNullOrWhiteSpace(content))
        {
            var addDataBuilder = new StringBuilder();
            AppendTlv(addDataBuilder, "08", content);
            AppendTlv(sb, "62", addDataBuilder.ToString());
        }

        // Tag 63: CRC16-CCITT Checksum
        sb.Append("6304");
        var crc = CalculateCrc16Ccitt(Encoding.ASCII.GetBytes(sb.ToString()));
        sb.Append(crc.ToString("X4"));

        return sb.ToString();
    }

    private static void AppendTlv(StringBuilder sb, string tag, string value)
    {
        sb.Append(tag);
        sb.Append(value.Length.ToString("D2"));
        sb.Append(value);
    }

    private static ushort CalculateCrc16Ccitt(byte[] bytes)
    {
        ushort crc = 0xFFFF;
        foreach (var b in bytes)
        {
            crc ^= (ushort)(b << 8);
            for (int i = 0; i < 8; i++)
            {
                if ((crc & 0x8000) != 0)
                    crc = (ushort)((crc << 1) ^ 0x1021);
                else
                    crc <<= 1;
            }
        }
        return crc;
    }

    #endregion
}
