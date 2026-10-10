using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Domain.Common;

namespace NovaLive.Infrastructure.Auth;

public sealed class LoggingEmailSender(IHostEnvironment environment, ILogger<LoggingEmailSender> logger) : IEmailSender
{
    public Task SendOtpAsync(string email, string otp, OtpType type, CancellationToken cancellationToken = default)
    {
        if (!environment.IsDevelopment()) throw new InvalidOperationException("LoggingEmailSender is Development-only.");
        // Keep the existing no-secret logging policy, including in Development.
        logger.LogWarning("Development email stub: OTP delivery suppressed for purpose {OtpType}. Configure Email:Provider to receive codes.", type);
        return Task.CompletedTask;
    }
}
