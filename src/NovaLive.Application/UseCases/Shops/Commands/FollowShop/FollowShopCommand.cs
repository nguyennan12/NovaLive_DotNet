using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Shops;
using NovaLive.Domain.Common;
using NovaLive.Domain.Shops;

namespace NovaLive.Application.UseCases.Shops.Commands.FollowShop;

[RequireAuthenticated]
public sealed record FollowShopCommand(Guid ShopId) : ICommand, ITransactionalCommand, IRequirePermission
{
    public string RequiredPermission => NovaLive.Domain.Rbac.PermissionCodes.Shops.Follow;
}
