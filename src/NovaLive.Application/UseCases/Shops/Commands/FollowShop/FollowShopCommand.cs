using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;

namespace NovaLive.Application.UseCases.Shops.Commands.FollowShop;

[RequireAuthenticated]
public sealed record FollowShopCommand(Guid ShopId) : ICommand, ITransactionalCommand, IRequirePermission
{
    public string RequiredPermission => NovaLive.Domain.Rbac.PermissionCodes.Shops.Follow;
}
