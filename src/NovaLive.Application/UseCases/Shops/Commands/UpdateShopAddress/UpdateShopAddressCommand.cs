using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Shops;

namespace NovaLive.Application.UseCases.Shops.Commands.UpdateShopAddress;

[AuthorizeRole("Seller")]
public sealed record UpdateShopAddressCommand(Guid Id, CreateShopAddressRequest Request) : ICommand<ShopAddressResponse>, ITransactionalCommand, IRequirePermission
{
    public string RequiredPermission => NovaLive.Domain.Rbac.PermissionCodes.Shops.ManageOwn;
}
