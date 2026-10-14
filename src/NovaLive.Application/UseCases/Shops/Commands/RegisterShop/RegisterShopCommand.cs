using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Shops;

namespace NovaLive.Application.UseCases.Shops.Commands.RegisterShop;

[RequireAuthenticated]
public sealed record RegisterShopCommand(RegisterShopRequest Request) : ICommand<ShopResponse>, ITransactionalCommand, IRequirePermission
{
    public string RequiredPermission => NovaLive.Domain.Rbac.PermissionCodes.Shops.Register;
}
