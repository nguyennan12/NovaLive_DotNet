namespace NovaLive.Application.Abstractions.Auth;

public interface ICurrentUser
{
    Guid? UserId { get; }

    string? Email { get; }

    Guid? ShopId { get; }

    IReadOnlyCollection<string> Roles { get; }

    IReadOnlyCollection<string> Permissions { get; }

    bool IsAuthenticated { get; }

    bool HasPermission(string permission);

    bool IsInRole(string role);
}
