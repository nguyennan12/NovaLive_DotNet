using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;

namespace NovaLive.Application.UseCases.Shops.Commands.DeleteShopAddress;

[AuthorizeRole("Seller")]
public sealed record DeleteShopAddressCommand(Guid Id) : ICommand, ITransactionalCommand, IRequirePermission
{
    public string RequiredPermission => NovaLive.Domain.Rbac.PermissionCodes.Shops.ManageOwn;
}
