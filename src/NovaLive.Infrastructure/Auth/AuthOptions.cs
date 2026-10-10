using System.Text;
namespace NovaLive.Infrastructure.Auth;

public sealed class JwtOptions
{
    public string Secret { get; set; } = "";
    public string Issuer { get; set; } = "";
    public string Audience { get; set; } = "";
    public bool IsValid() => Encoding.UTF8.GetByteCount(Secret) >= 32
        && !string.IsNullOrWhiteSpace(Issuer) && !string.IsNullOrWhiteSpace(Audience);
}
public sealed class OtpOptions
{
    public string Pepper { get; set; } = "";
}
public sealed class SmtpOptions
{
    public string Host { get; set; } = "";
    public int Port { get; set; } = 587;
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string From { get; set; } = "";
    public bool UseSsl { get; set; }
    public bool UseLoggingSender { get; set; }
}
public sealed class GoogleOptions
{
    public bool Enabled { get; set; }
    public string ClientId { get; set; } = "";
}

