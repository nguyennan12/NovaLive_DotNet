using Microsoft.Extensions.Logging;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Domain.Common;

namespace NovaLive.Infrastructure.Auth;

public sealed class CompositeEmailSender(IEnumerable<IEmailSender> senders,
    ILogger<CompositeEmailSender> logger) : IEmailSender
{
    public async Task SendOtpAsync(string email, string otp, OtpType type, CancellationToken cancellationToken = default)
    {
        foreach (var sender in senders)
        {
            try
            {
                await sender.SendOtpAsync(email, otp, type, cancellationToken);
            }
            catch (Exception exception)
            {
                // Provider exceptions can contain OTPs or credentials. Record only safe metadata.
                logger.LogError("OTP email sender {SenderType} failed; failure type {FailureType}. Continuing with remaining senders.",
                    sender.GetType().Name, exception.GetType().Name);
            }
        }
    }
}
