using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Shops;
using NovaLive.Domain.Rbac;

namespace NovaLive.Application.UseCases.Shops.Queries.GetSellerShop;

[AuthorizeRole("Seller")]
public sealed record GetSellerShopQuery : IQuery<ShopResponse>, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Shops.ManageOwn;
}
