using NovaLive.Domain.Common;

namespace NovaLive.Domain.Users;

public sealed class UserOtp : Entity
{
    private UserOtp() { }
    public UserOtp(Guid userId, OtpType type, string hash, DateTimeOffset now)
    {
        UserId = userId;
        OtpType = type;
        OtpHash = hash;
        CreatedAt = now;
        ExpiresAt = now.AddMinutes(5);
    }
    public void MarkUsed(DateTimeOffset now) => UsedAt = now;
    public Guid UserId { get; private set; }

    public OtpType OtpType { get; private set; }

    public string OtpHash { get; private set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset? UsedAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
}
