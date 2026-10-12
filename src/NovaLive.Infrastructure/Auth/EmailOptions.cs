namespace NovaLive.Infrastructure.Auth;

public sealed class EmailOptions
{
    public string Provider { get; set; } = "Logging";
    public string ApiKey { get; set; } = "";
    public string FromAddress { get; set; } = "";
    public string FromName { get; set; } = "";
    public bool LogOtp { get; set; }
    public bool AllowOtpLoggingInProduction { get; set; }

    public bool IsResend => Provider.Equals("Resend", StringComparison.OrdinalIgnoreCase);
    public bool IsLogging => Provider.Equals("Logging", StringComparison.OrdinalIgnoreCase);
    public bool IsBoth => Provider.Equals("Both", StringComparison.OrdinalIgnoreCase);
    public bool RequiresResend => IsResend || IsBoth;
}
