namespace NovaLive.Application.Abstractions.CurrentUser;

public interface ICurrentUser
{
    Guid? UserId { get; }

    Guid? ShopId { get; }

    IReadOnlyCollection<string> Roles { get; }
}
