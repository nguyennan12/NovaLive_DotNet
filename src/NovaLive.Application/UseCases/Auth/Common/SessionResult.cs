using NovaLive.Contracts.V1.Auth;

namespace NovaLive.Application.UseCases.Auth.Common;

// Internal application result. Controllers expose only AuthResponse and deliver tokens via cookies.
public sealed record SessionResult(string AccessToken, string RefreshToken, DateTime ExpiresAt, UserSummaryDto User)
{
    public TimeSpan AccessTokenTtl { get; init; }
    public AuthResponse Response => new(ExpiresAt, User, AccessToken);
}
