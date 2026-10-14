using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Shops;

namespace NovaLive.Application.UseCases.Shops.Queries.GetShopAddresses;

[AuthorizeRole("Seller")]
public sealed record GetShopAddressesQuery() : IQuery<List<ShopAddressResponse>>, IRequirePermission
{
    public string RequiredPermission => NovaLive.Domain.Rbac.PermissionCodes.Shops.ManageOwn;
}
