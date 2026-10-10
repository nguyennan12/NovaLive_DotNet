using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
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
            html = $"<html lang=\"vi\"><body><p>Mã OTP để {purpose} của bạn:</p><p><strong>{WebUtility.HtmlEncode(otp)}</strong></p><p>Mã có hiệu lực trong 5 phút. Vui lòng không chia sẻ mã này với người khác.</p></body></html>"
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
                if (response.IsSuccessStatusCode) return;
                var status = (int)response.StatusCode;
                var retry = status >= 500 && status <= 599 && attempt == 1;
                if (!retry)
                {
                    logger.LogError("OTP email delivery failed with HTTP {StatusCode} after {Attempts} attempt(s).", status, attempt);
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
                // Never attach provider exceptions, response bodies, headers, email addresses or codes to logs.
                logger.LogError("OTP email delivery failed due to a transport error.");
                throw new InvalidOperationException("Email delivery failed.");
            }
            await Task.Delay(TimeSpan.FromMilliseconds(250), cancellationToken);
        }
    }
}
