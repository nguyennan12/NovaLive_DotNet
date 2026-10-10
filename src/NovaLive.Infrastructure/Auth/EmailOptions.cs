using Microsoft.Extensions.Hosting;

namespace NovaLive.Infrastructure.Auth;

public sealed class EmailOptions
{
    public string Provider { get; set; } = "";
    public string ApiKey { get; set; } = "";
    public string FromAddress { get; set; } = "";
    public string FromName { get; set; } = "";

    public bool IsResend => Provider.Equals("Resend", StringComparison.OrdinalIgnoreCase);
    public bool UseLogging(IHostEnvironment environment) => environment.IsDevelopment()
        && (string.IsNullOrWhiteSpace(Provider) || Provider.Equals("Logging", StringComparison.OrdinalIgnoreCase));
}
