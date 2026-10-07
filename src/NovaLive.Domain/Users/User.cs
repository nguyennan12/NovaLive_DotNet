using NovaLive.Domain.Common;

namespace NovaLive.Domain.Users;

public sealed class User : AuditableEntity
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

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string PasswordHash { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public DateOnly? Birthday { get; set; }

    public UserGender? Gender { get; set; }

    public string? AvatarUrl { get; set; }

    public bool IsSeller { get; set; }

    public AccountStatus AccountStatus { get; set; } = AccountStatus.Unverified;

    public DateTimeOffset? DeletedAt { get; set; }

    public void Activate()
    {
        AccountStatus = AccountStatus.Active;
    }

    public void MarkAsSeller()
    {
        IsSeller = true;
    }
}
