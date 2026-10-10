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
