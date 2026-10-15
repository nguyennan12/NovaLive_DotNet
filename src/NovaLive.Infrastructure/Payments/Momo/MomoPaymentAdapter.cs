using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NovaLive.Application.Abstractions.ThirdParty;
using NovaLive.Contracts.V1.Payments;

namespace NovaLive.Infrastructure.Payments.Momo;

/// <summary>
/// Bộ điều hợp thanh toán tích hợp cổng MoMo API v2 và xác thực chữ ký IPN HMAC-SHA256.
/// </summary>
public sealed class MomoPaymentAdapter : IMomoPaymentAdapter
{
    private readonly HttpClient _httpClient;
    private readonly MomoOptions _options;
    private readonly ILogger<MomoPaymentAdapter> _logger;

    public string GatewayName => "MoMo";

    public MomoPaymentAdapter(
        HttpClient httpClient,
        IOptions<MomoOptions> options,
        ILogger<MomoPaymentAdapter> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Sinh chữ ký HMAC-SHA256 và gửi yêu cầu tạo link thanh toán tới MoMo API.
    /// </summary>
    public async Task<MomoCreatePaymentResponse> CreatePaymentUrlAsync(
        string orderId,
        long amount,
        string orderInfo,
        string? extraData = null,
        CancellationToken ct = default)
    {
        var requestId = Guid.NewGuid().ToString("N");
        var actualExtraData = extraData ?? string.Empty;
        var requestType = _options.RequestType;

        // Định dạng chuỗi ký thô theo tài liệu chính thức MoMo:
        // accessKey=$accessKey&amount=$amount&extraData=$extraData&ipnUrl=$ipnUrl&orderId=$orderId&orderInfo=$orderInfo&partnerCode=$partnerCode&redirectUrl=$redirectUrl&requestId=$requestId&requestType=$requestType
        var rawSignature =
            $"accessKey={_options.AccessKey}&amount={amount}&extraData={actualExtraData}&ipnUrl={_options.IpnUrl}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={_options.PartnerCode}&redirectUrl={_options.RedirectUrl}&requestId={requestId}&requestType={requestType}";

        var signature = ComputeHmacSha256(rawSignature, _options.SecretKey);

        var requestBody = new MomoCreatePaymentRequest(
            PartnerCode: _options.PartnerCode,
            RequestId: requestId,
            Amount: amount,
            OrderId: orderId,
            OrderInfo: orderInfo,
            RedirectUrl: _options.RedirectUrl,
            IpnUrl: _options.IpnUrl,
            RequestType: requestType,
            ExtraData: actualExtraData,
            Lang: "vi",
            Signature: signature);

        _logger.LogInformation("Gửi yêu cầu tạo thanh toán MoMo: OrderId={OrderId}, Amount={Amount}", orderId, amount);

        var response = await _httpClient.PostAsJsonAsync(_options.Endpoint, requestBody, ct);
        var responseContent = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Lỗi gọi MoMo API ({StatusCode}): {Response}", response.StatusCode, responseContent);
            throw new HttpRequestException($"Lỗi kết nối tới MoMo: {response.StatusCode} - {responseContent}");
        }

        var result = JsonSerializer.Deserialize<MomoCreatePaymentResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (result is null)
        {
            throw new InvalidOperationException("Không thể đọc phản hồi từ MoMo API.");
        }

        return result;
    }

    /// <summary>
    /// Kiểm tra tính hợp lệ của chữ ký HMAC-SHA256 gửi trong IPN từ MoMo.
    /// </summary>
    public bool VerifySignature(MomoIpnRequest ipnRequest)
    {
        if (string.IsNullOrWhiteSpace(ipnRequest.Signature))
        {
            return false;
        }

        // Định dạng chuỗi ký IPN theo chuẩn MoMo:
        // accessKey=$accessKey&amount=$amount&extraData=$extraData&message=$message&orderId=$orderId&orderInfo=$orderInfo&orderType=$orderType&partnerCode=$partnerCode&payType=$payType&requestId=$requestId&responseTime=$responseTime&resultCode=$resultCode&transId=$transId
        var rawSignature =
            $"accessKey={_options.AccessKey}&amount={ipnRequest.Amount}&extraData={ipnRequest.ExtraData}&message={ipnRequest.Message}&orderId={ipnRequest.OrderId}&orderInfo={ipnRequest.OrderInfo}&orderType={ipnRequest.OrderType}&partnerCode={ipnRequest.PartnerCode}&payType={ipnRequest.PayType}&requestId={ipnRequest.RequestId}&responseTime={ipnRequest.ResponseTime}&resultCode={ipnRequest.ResultCode}&transId={ipnRequest.TransId}";

        var expectedSignature = ComputeHmacSha256(rawSignature, _options.SecretKey);

        var expectedBytes = Encoding.UTF8.GetBytes(expectedSignature);
        var actualBytes = Encoding.UTF8.GetBytes(ipnRequest.Signature);

        if (expectedBytes.Length != actualBytes.Length)
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(expectedBytes, actualBytes);
    }

    /// <summary>
    /// Tạo PaymentIntent chuẩn IPaymentGatewayAdapter.
    /// </summary>
    public async Task<PaymentIntentResult> CreatePaymentIntentAsync(
        PaymentIntentRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await CreatePaymentUrlAsync(
            request.PaymentId.ToString(),
            (long)request.Amount,
            request.Description,
            null,
            cancellationToken);

        return new PaymentIntentResult(response.OrderId, response.PayUrl, response.QrCodeUrl);
    }

    /// <summary>
    /// Hàm tiện ích tính mã băm HMAC-SHA256 ra chuỗi Hex chữ thường.
    /// </summary>
    public static string ComputeHmacSha256(string message, string secretKey)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        var messageBytes = Encoding.UTF8.GetBytes(message);

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(messageBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
