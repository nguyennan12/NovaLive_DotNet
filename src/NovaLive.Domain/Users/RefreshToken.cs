using NovaLive.Domain.Common;

namespace NovaLive.Domain.Users;

public sealed class RefreshToken : Entity
{
    public Guid UserId { get; private set; }

    public string TokenHash { get; private set; } = string.Empty;

    public string? DeviceInfo { get; private set; }

    public string? IpAddress { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset? RevokedAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
}
