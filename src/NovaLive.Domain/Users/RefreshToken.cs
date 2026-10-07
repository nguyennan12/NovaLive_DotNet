using NovaLive.Domain.Common;

namespace NovaLive.Domain.Users;

public sealed class RefreshToken : Entity
{
    public Guid UserId { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    public string? DeviceInfo { get; set; }

    public string? IpAddress { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
