using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Shops;
using NovaLive.Domain.Rbac;

namespace NovaLive.Application.UseCases.Shops.Commands.ApproveShop;

[AuthorizeRole("Admin")]
public sealed record ApproveShopCommand(Guid ShopId, ApproveShopRequest Request)
    : ICommand<ShopResponse>, ITransactionalCommand, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Shops.ApproveKyc;
}
