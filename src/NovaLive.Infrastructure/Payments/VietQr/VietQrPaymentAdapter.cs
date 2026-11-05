using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NovaLive.Application.Abstractions.ThirdParty;
using NovaLive.Contracts.V1.Payments;
using QRCoder;

namespace NovaLive.Infrastructure.Payments.VietQr;

public sealed class VietQrPaymentAdapter : IVietQrPaymentAdapter
{
    private static readonly Regex OrderCodeRegex = new(@"(?:NOVA|VQR)[\s_-]*([A-Za-z0-9_-]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly VietQrOptions _options;
    private readonly HttpClient _http;
    private readonly ILogger<VietQrPaymentAdapter> _logger;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);
    private string? _providerToken;
    private DateTimeOffset _providerTokenExpiresAt;

    public VietQrPaymentAdapter(HttpClient http, IOptions<VietQrOptions> options, ILogger<VietQrPaymentAdapter> logger)
    {
        _http = http;
        _options = options.Value;
        _logger = logger;
    }

    public string GatewayName => "VietQR";

    public async Task<VietQrResponse> GenerateDynamicQrAsync(string orderCode, decimal amount, string? description = null, CancellationToken cancellationToken = default)
    {
        EnsureProviderConfiguration();
        if (amount <= 0 || amount != decimal.Truncate(amount) || amount > long.MaxValue)
            throw new InvalidOperationException("Số tiền VietQR phải là số nguyên dương hợp lệ.");

        // The callback is correlated by this exact transfer content. Do not let a display
        // description replace it, otherwise a successful bank transfer cannot settle the order.
        var content = BuildContent(orderCode);
        var token = await GetProviderTokenAsync(cancellationToken);
        using var request = new HttpRequestMessage(HttpMethod.Post, BuildProviderUrl("/vqr/api/qr/generate-customer"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(new
        {
            bankCode = _options.BankCode,
            bankAccount = _options.AccountNumber,
            userBankName = _options.AccountName,
            content,
            qrType = 0,
            amount = (long)amount,
            orderId = CreateProviderOrderId(orderCode),
            transType = "C",
            additionalData = Array.Empty<object>()
        });

        using var response = await _http.SendAsync(request, cancellationToken);
        var raw = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"VietQR Generate QR thất bại ({(int)response.StatusCode}): {ReadProviderMessage(raw)}");

        var provider = JsonSerializer.Deserialize<GenerateQrResponse>(raw, Json)
            ?? throw new InvalidOperationException("VietQR trả về dữ liệu tạo QR không hợp lệ.");
        if (string.IsNullOrWhiteSpace(provider.QrCode)) throw new InvalidOperationException("VietQR không trả về qrCode.");
        var qrImage = Convert.ToBase64String(PngByteQRCodeHelper.GetQRCode(provider.QrCode, QRCodeGenerator.ECCLevel.M, 8));
        return new VietQrResponse(provider.QrCode, provider.QrLink ?? string.Empty,
            provider.UserBankName ?? _options.AccountName, provider.BankAccount ?? _options.AccountNumber,
            provider.BankCode ?? _options.BankCode, ParseAmount(provider.Amount, amount), provider.Content ?? content, qrImage);
    }

    public async Task<VietQrTestCallbackResponse> TriggerTestCallbackAsync(VietQrResponse qr, CancellationToken cancellationToken = default)
    {
        var baseUri = new Uri(_options.BaseUrl, UriKind.Absolute);
        if (!string.Equals(baseUri.Host, "dev.vietqr.org", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Test Callback chỉ được phép với môi trường Sandbox dev.vietqr.org.");

        var token = await GetProviderTokenAsync(cancellationToken);
        using var request = new HttpRequestMessage(HttpMethod.Post, BuildProviderUrl("/vqr/bank/api/test/transaction-callback"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(new
        {
            bankAccount = qr.AccountNumber,
            content = qr.Description,
            amount = (long)qr.Amount,
            transType = "C",
            bankCode = _options.BankCode
        });
        using var response = await _http.SendAsync(request, cancellationToken);
        var raw = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<VietQrTestCallbackResponse>(raw, Json)
            ?? new VietQrTestCallbackResponse("FAILED", ReadProviderMessage(raw));
        if (!response.IsSuccessStatusCode || !string.Equals(result.Status, "SUCCESS", StringComparison.OrdinalIgnoreCase))
            throw new HttpRequestException($"VietQR Test Callback thất bại ({(int)response.StatusCode}): {result.Message}");
        return result;
    }

    public bool TryIssueCallbackToken(string username, string password, out VietQrTokenResponse? response)
    {
        response = null;
        if (!IsCallbackConfigurationValid() || !FixedTimeEquals(username, _options.CallbackUsername)
            || !FixedTimeEquals(password, _options.CallbackPassword)) return false;
        var now = DateTimeOffset.UtcNow;
        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.CallbackJwtSecret)), SecurityAlgorithms.HmacSha256);
        var jwt = new JwtSecurityToken(
            claims: [new Claim(JwtRegisteredClaimNames.Sub, username), new Claim("scope", "vietqr:transaction-sync")],
            notBefore: now.UtcDateTime,
            expires: now.AddSeconds(_options.CallbackTokenLifetimeSeconds).UtcDateTime,
            signingCredentials: credentials);
        response = new VietQrTokenResponse(new JwtSecurityTokenHandler { MapInboundClaims = false }.WriteToken(jwt), "Bearer", _options.CallbackTokenLifetimeSeconds);
        return true;
    }

    public bool VerifyCallbackToken(string token)
    {
        if (!IsCallbackConfigurationValid() || string.IsNullOrWhiteSpace(token)) return false;
        try
        {
            var principal = new JwtSecurityTokenHandler { MapInboundClaims = false }.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.CallbackJwtSecret)),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                ValidAlgorithms = [SecurityAlgorithms.HmacSha256]
            }, out _);
            return principal.HasClaim("scope", "vietqr:transaction-sync")
                && string.Equals(principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, _options.CallbackUsername, StringComparison.Ordinal);
        }
        catch (Exception exception) when (exception is SecurityTokenException or ArgumentException) { return false; }
    }

    public bool IsExpectedReceivingAccount(string? bankAccount) =>
        !string.IsNullOrWhiteSpace(bankAccount) && FixedTimeEquals(bankAccount.Trim(), _options.AccountNumber.Trim());

    public bool IsExpectedOrderId(string orderCode, string? providerOrderId) =>
        !string.IsNullOrWhiteSpace(providerOrderId)
        && FixedTimeEquals(CreateProviderOrderId(orderCode), providerOrderId.Trim().ToUpperInvariant());

    public bool VerifyWebhookSignature(string rawPayload, string? signature)
    {
        if (string.IsNullOrWhiteSpace(_options.WebhookSecret) || string.IsNullOrWhiteSpace(signature)) return false;
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.WebhookSecret));
        var computed = Convert.ToHexStringLower(hmac.ComputeHash(Encoding.UTF8.GetBytes(rawPayload)));
        return FixedTimeEquals(signature.ToLowerInvariant(), computed);
    }

    public string? ExtractOrderCode(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return null;
        var match = OrderCodeRegex.Match(content);
        if (match.Success && match.Groups.Count > 1) return match.Groups[1].Value.Trim();
        var trimmed = content.Trim();
        return !trimmed.Contains(' ') && trimmed.Length >= 4 ? trimmed : null;
    }

    public async Task<PaymentIntentResult> CreatePaymentIntentAsync(PaymentIntentRequest request, CancellationToken cancellationToken = default)
    {
        var response = await GenerateDynamicQrAsync(request.PaymentId.ToString("N"), request.Amount, request.Description, cancellationToken);
        return new PaymentIntentResult(response.Description, response.QrDataUrl, response.QrCode);
    }

    private async Task<string> GetProviderTokenAsync(CancellationToken cancellationToken)
    {
        if (_providerToken is not null && _providerTokenExpiresAt > DateTimeOffset.UtcNow.AddSeconds(15)) return _providerToken;
        await _tokenLock.WaitAsync(cancellationToken);
        try
        {
            if (_providerToken is not null && _providerTokenExpiresAt > DateTimeOffset.UtcNow.AddSeconds(15)) return _providerToken;
            var basic = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.ClientId}:{_options.ApiKey}"));
            using var request = new HttpRequestMessage(HttpMethod.Post, BuildProviderUrl("/vqr/api/token_generate"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", basic);
            using var response = await _http.SendAsync(request, cancellationToken);
            var raw = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"VietQR Get Token thất bại ({(int)response.StatusCode}): {ReadProviderMessage(raw)}");
            var token = JsonSerializer.Deserialize<VietQrTokenResponse>(raw, Json);
            if (token is null || string.IsNullOrWhiteSpace(token.AccessToken) || token.ExpiresIn <= 0)
                throw new InvalidOperationException("VietQR trả về access token không hợp lệ.");
            _providerToken = token.AccessToken;
            _providerTokenExpiresAt = DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn);
            return _providerToken;
        }
        finally { _tokenLock.Release(); }
    }

    private void EnsureProviderConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_options.ClientId) || string.IsNullOrWhiteSpace(_options.ApiKey)
            || string.IsNullOrWhiteSpace(_options.BaseUrl) || string.IsNullOrWhiteSpace(_options.BankCode)
            || string.IsNullOrWhiteSpace(_options.AccountNumber) || string.IsNullOrWhiteSpace(_options.AccountName))
            throw new InvalidOperationException("VietQR chưa được cấu hình đủ username, password, bankCode và tài khoản nhận tiền.");
    }

    private bool IsCallbackConfigurationValid() => !string.IsNullOrWhiteSpace(_options.CallbackUsername)
        && !string.IsNullOrWhiteSpace(_options.CallbackPassword)
        && Encoding.UTF8.GetByteCount(_options.CallbackJwtSecret) >= 32
        && _options.CallbackTokenLifetimeSeconds > 0;

    private string BuildProviderUrl(string path) => $"{_options.BaseUrl.TrimEnd('/')}{path}";

    private static string BuildContent(string orderCode)
    {
        var content = $"NOVA {orderCode.Trim()}";
        if (content.Length > 23 || content.Any(character => character > 127 || (!char.IsLetterOrDigit(character) && character != ' ' && character != '-')))
            throw new InvalidOperationException("Nội dung VietQR tối đa 23 ký tự, không dấu và không chứa ký tự đặc biệt.");
        return content;
    }

    private static string CreateProviderOrderId(string orderCode) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(orderCode)))[..13];
    private static decimal ParseAmount(string? value, decimal fallback) => decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed) ? parsed : fallback;

    private static string ReadProviderMessage(string raw)
    {
        try
        {
            using var document = JsonDocument.Parse(raw);
            return document.RootElement.TryGetProperty("message", out var message) ? message.GetString() ?? raw : raw;
        }
        catch (JsonException) { return raw; }
    }

    private static bool FixedTimeEquals(string left, string right)
    {
        var leftBytes = Encoding.UTF8.GetBytes(left);
        var rightBytes = Encoding.UTF8.GetBytes(right);
        return leftBytes.Length == rightBytes.Length && CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
    }

    private sealed record GenerateQrResponse(string? BankCode, string? BankAccount, string? UserBankName, string? Amount, string? Content, string? QrCode, string? QrLink);
}
