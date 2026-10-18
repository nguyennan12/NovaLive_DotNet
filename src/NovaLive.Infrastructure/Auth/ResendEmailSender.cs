using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Domain.Common;

namespace NovaLive.Infrastructure.Auth;

public sealed class ResendEmailSender(HttpClient client, IOptions<EmailOptions> options,
    ILogger<ResendEmailSender> logger) : IEmailSender
{
    public async Task SendOtpAsync(string email, string otp, OtpType type, CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        var purpose = type == OtpType.EmailVerify ? "xác thực tài khoản" : "đặt lại mật khẩu";
        var payload = new
        {
            from = string.IsNullOrWhiteSpace(settings.FromName) ? settings.FromAddress
                : $"{settings.FromName} <{settings.FromAddress}>",
            to = new[] { email },
            subject = $"NovaLive — Mã {purpose}",
            html = OtpEmailTemplate.Render(otp, type, purpose)
        };
        // A retry after timeout must not deliver a duplicate message if the first request was accepted.
        var idempotencyKey = Guid.NewGuid().ToString("N");
        for (var attempt = 1; attempt <= 2; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);
                request.Headers.Add("Idempotency-Key", idempotencyKey);
                request.Content = JsonContent.Create(payload);
                using var response = await client.SendAsync(request, cancellationToken);
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    logger.LogInformation("Resend OTP email accepted; message id {MessageId}.",
                        Redact(GetMessageId(responseBody), otp, settings.ApiKey));
                    return;
                }
                var status = (int)response.StatusCode;
                logger.LogError("Resend OTP email delivery failed with HTTP {StatusCode} on attempt {Attempt}; response body: {ResponseBody}",
                    status, attempt, Redact(responseBody, otp, settings.ApiKey));
                var retry = status >= 500 && status <= 599 && attempt == 1;
                if (!retry)
                {
                    throw new InvalidOperationException("Email delivery failed.");
                }
                logger.LogWarning("OTP email delivery received HTTP {StatusCode}; retrying once.", status);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                if (attempt == 2)
                {
                    logger.LogError("OTP email delivery timed out after {Attempts} attempts.", attempt);
                    throw new TimeoutException("Email delivery timed out.");
                }
                logger.LogWarning("OTP email delivery timed out; retrying once.");
            }
            catch (HttpRequestException)
            {
                // Never attach provider exceptions or headers: they can contain credentials.
                logger.LogError("OTP email delivery failed due to a transport error.");
                throw new InvalidOperationException("Email delivery failed.");
            }
            await Task.Delay(TimeSpan.FromMilliseconds(250), cancellationToken);
        }
    }

    private static string GetMessageId(string body)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            if (document.RootElement.ValueKind == JsonValueKind.Object
                && document.RootElement.TryGetProperty("id", out var id) && id.ValueKind == JsonValueKind.String)
                return id.GetString() ?? "unavailable";
        }
        catch (JsonException) { }
        return "unavailable";
    }

    private static string Redact(string value, params string[] secrets)
    {
        // Resend may echo request data in its error body. Keep diagnostics but remove known secrets.
        foreach (var secret in secrets.Where(secret => !string.IsNullOrEmpty(secret)))
        {
            value = value.Replace(JsonSerializer.Serialize(secret)[1..^1], "[REDACTED]", StringComparison.Ordinal)
                .Replace(WebUtility.HtmlEncode(secret), "[REDACTED]", StringComparison.Ordinal)
                .Replace(Uri.EscapeDataString(secret), "[REDACTED]", StringComparison.Ordinal)
                .Replace(secret, "[REDACTED]", StringComparison.Ordinal);
        }
        return value;
    }
}
