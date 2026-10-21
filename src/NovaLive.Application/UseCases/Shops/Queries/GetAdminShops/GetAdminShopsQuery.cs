using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Shops;
using NovaLive.Domain.Rbac;

namespace NovaLive.Application.UseCases.Shops.Queries.GetAdminShops;

[AuthorizeRole("Admin")]
public sealed record GetAdminShopsQuery(GetAdminShopsRequest Request)
    : IQuery<PagedResult<ShopResponse>>, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Shops.ApproveKyc;
}
