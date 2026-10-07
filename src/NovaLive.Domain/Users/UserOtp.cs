using NovaLive.Domain.Common;

namespace NovaLive.Domain.Users;

public sealed class UserOtp : Entity
{
    public Guid UserId { get; private set; }

    public OtpType OtpType { get; private set; }

    public string OtpHash { get; private set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset? UsedAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
}
