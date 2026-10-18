using Microsoft.Extensions.Logging;
using NovaLive.Application.Abstractions.Services;

namespace NovaLive.Infrastructure.Auth;

public sealed class LoggingOrderConfirmationEmailSender(ILogger<LoggingOrderConfirmationEmailSender> logger)
    : IOrderConfirmationEmailSender
{
    public Task SendAsync(string email, string html, CancellationToken ct = default)
    {
        logger.LogInformation("Order confirmation email suppressed because Email:Provider is Logging.");
        return Task.CompletedTask;
    }
}
