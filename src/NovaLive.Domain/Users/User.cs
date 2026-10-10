using NovaLive.Domain.Common;

namespace NovaLive.Domain.Users;

public sealed class User : AuditableEntity, ISoftDeletable
{
    private User()
    {
    }

    public User(string email, string passwordHash, string fullName, string? phone = null)
    {
        Email = email;
        PasswordHash = passwordHash;
        FullName = fullName;
        Phone = phone;
    }

    public string Email { get; private set; } = string.Empty;

    public string? Phone { get; private set; }

    public string PasswordHash { get; private set; } = string.Empty;

    public string FullName { get; private set; } = string.Empty;

    public DateOnly? Birthday { get; private set; }

    public UserGender? Gender { get; private set; }

    public string? AvatarUrl { get; private set; }

    public bool IsSeller { get; private set; }

    public AccountStatus AccountStatus { get; private set; } = AccountStatus.Unverified;

    public DateTimeOffset? DeletedAt { get; private set; }

    public void Activate()
    {
        AccountStatus = AccountStatus.Active;
    }

    public void ChangePassword(string passwordHash) => PasswordHash = passwordHash;

    public void MarkAsSeller()
    {
        IsSeller = true;
    }
}
