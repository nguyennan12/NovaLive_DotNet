using NovaLive.Domain.Common;

namespace NovaLive.Domain.Users;

public sealed class UserOtp : Entity
{
    public Guid UserId { get; set; }

    public OtpType OtpType { get; set; }

    public string OtpHash { get; set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset? UsedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
