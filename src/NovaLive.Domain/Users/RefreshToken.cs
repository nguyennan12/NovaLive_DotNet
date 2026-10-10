using NovaLive.Domain.Common;

namespace NovaLive.Domain.Users;

public sealed class RefreshToken : Entity
{
    private RefreshToken() { }
    public RefreshToken(Guid userId, string hash, string? deviceInfo, string? ipAddress, DateTimeOffset now)
    {
        UserId = userId;
        TokenHash = hash;
        DeviceInfo = deviceInfo is { Length: > 500 } ? deviceInfo[..500] : deviceInfo;
        IpAddress = ipAddress is { Length: > 45 } ? ipAddress[..45] : ipAddress;
        CreatedAt = now;
        ExpiresAt = now.AddDays(30);
    }
    public void Revoke(DateTimeOffset now) => RevokedAt ??= now;
    public Guid UserId { get; private set; }

    public string TokenHash { get; private set; } = string.Empty;

    public string? DeviceInfo { get; private set; }

    public string? IpAddress { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset? RevokedAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
}
