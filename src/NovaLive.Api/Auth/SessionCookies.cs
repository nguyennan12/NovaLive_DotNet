using Microsoft.Extensions.Options;
using NovaLive.Application.UseCases.Auth.Common;

namespace NovaLive.Api.Auth;

public sealed class SessionCookieOptions
{
    public string AccessTokenName { get; set; } = "nova_at";
    public string RefreshTokenName { get; set; } = "nova_rt";
    public string SameSite { get; set; } = "Lax";
    public bool Secure { get; set; } = true;
    public string? Domain { get; set; }

    public bool IsValid() => ValidName(AccessTokenName) && ValidName(RefreshTokenName)
        && AccessTokenName != RefreshTokenName && SameSite is "Lax" or "Strict" or "None";
    private static bool ValidName(string name) => !string.IsNullOrWhiteSpace(name)
        && System.Text.RegularExpressions.Regex.IsMatch(name, "^[A-Za-z0-9_-]+$")
        && !name.StartsWith("__Host-", StringComparison.Ordinal);
}

public sealed class SessionCookies(IOptions<SessionCookieOptions> options, IHostEnvironment environment)
{
    public string? ReadRefresh(HttpRequest request) => request.Cookies[options.Value.RefreshTokenName];

    private CookieOptions Create(string path) => new()
    {
        HttpOnly = true,
        Secure = !environment.IsDevelopment() || options.Value.Secure || options.Value.SameSite == "None",
        SameSite = Enum.Parse<SameSiteMode>(options.Value.SameSite),
        Path = path,
        Domain = string.IsNullOrWhiteSpace(options.Value.Domain) ? null : options.Value.Domain,
        IsEssential = true
    };

    public void Write(HttpResponse response, SessionResult session)
    {
        var access = Create("/");
        access.MaxAge = session.AccessTokenTtl;
        access.Expires = new DateTimeOffset(session.ExpiresAt, TimeSpan.Zero);
        response.Cookies.Append(options.Value.AccessTokenName, session.AccessToken, access);
        var refresh = Create("/api/v1/auth");
        refresh.MaxAge = TimeSpan.FromDays(30);
        response.Cookies.Append(options.Value.RefreshTokenName, session.RefreshToken, refresh);
        response.Headers.CacheControl = "no-store";
        response.Headers.Pragma = "no-cache";
    }

    public void Clear(HttpResponse response)
    {
        response.Cookies.Delete(options.Value.AccessTokenName, Create("/"));
        response.Cookies.Delete(options.Value.RefreshTokenName, Create("/api/v1/auth"));
        response.Headers.CacheControl = "no-store";
    }
}
