using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Shops;

namespace NovaLive.Application.UseCases.Shops.Commands.UpdateShopInfo;

[AuthorizeRole("Seller")]
public sealed record UpdateShopInfoCommand(UpdateShopProfileRequest Request) : ICommand<ShopResponse>, ITransactionalCommand, IRequirePermission
{
    public string RequiredPermission => NovaLive.Domain.Rbac.PermissionCodes.Shops.ManageOwn;
}
