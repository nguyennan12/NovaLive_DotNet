using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;

namespace NovaLive.Application.UseCases.Shops.Commands.UnfollowShop;

[RequireAuthenticated]
public sealed record UnfollowShopCommand(Guid ShopId) : ICommand, ITransactionalCommand, IRequirePermission
{
    public string RequiredPermission => NovaLive.Domain.Rbac.PermissionCodes.Shops.Follow;
}
