using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Shops;
using NovaLive.Domain.Rbac;

namespace NovaLive.Application.UseCases.Wallets.Queries.GetAdminPayouts;

[AuthorizeRole("Admin")]
public sealed record GetAdminPayoutsQuery(GetAdminPayoutsRequest Request)
    : IQuery<PagedResult<SellerPayoutResponse>>, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Wallets.ApprovePayout;
}
