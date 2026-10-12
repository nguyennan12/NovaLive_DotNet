using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Domain.Common;

namespace NovaLive.Infrastructure.Auth;

public sealed class LoggingEmailSender(IHostEnvironment environment, IOptions<EmailOptions> options,
    ILogger<LoggingEmailSender> logger) : IEmailSender
{
    public Task SendOtpAsync(string email, string otp, OtpType type, CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        if (!settings.LogOtp) return Task.CompletedTask;
        if (environment.IsProduction() && !settings.AllowOtpLoggingInProduction)
        {
            logger.LogWarning("OTP logging suppressed in Production: Email:AllowOtpLoggingInProduction must be explicitly enabled.");
            return Task.CompletedTask;
        }
        // Explicitly opt-in for testing; never include the code in any other log path.
        logger.LogInformation("Test OTP for {Email}, purpose {OtpType}: {Otp}", email, type, otp);
        return Task.CompletedTask;
    }
}
