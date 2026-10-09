using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Domain.Common;
namespace NovaLive.Infrastructure.Auth;

public sealed class SmtpEmailSender(IOptions<SmtpOptions> options) : IEmailSender
{
    public async Task SendOtpAsync(string email, string otp, OtpType type, CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(settings.From));
        message.To.Add(MailboxAddress.Parse(email));
        message.Subject = type == OtpType.EmailVerify ? "NovaLive email verification" : "NovaLive password reset";
        message.Body = new TextPart("plain") { Text = $"Your NovaLive code is {otp}. It expires in 5 minutes." };
        using var client = new SmtpClient();
        await client.ConnectAsync(settings.Host, settings.Port,
            settings.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls, cancellationToken);
        if (!string.IsNullOrEmpty(settings.Username))
            await client.AuthenticateAsync(settings.Username, settings.Password, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
public sealed class LoggingEmailSender(IHostEnvironment environment, ILogger<LoggingEmailSender> logger) : IEmailSender
{
    public Task SendOtpAsync(string email, string otp, OtpType type, CancellationToken cancellationToken = default)
    {
        if (!environment.IsDevelopment()) throw new InvalidOperationException("LoggingEmailSender is Development-only.");
        // WARNING: This is a development stub; it does not deliver email. Never log the OTP itself.
        logger.LogWarning("Development email stub: OTP delivery suppressed for purpose {OtpType}. Configure SMTP to receive codes.", type);
        return Task.CompletedTask;
    }
}

