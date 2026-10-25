using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Dashboards;
using NovaLive.Domain.Rbac;

namespace NovaLive.Application.UseCases.Dashboards.Seller.GetSellerDashboard;

[AuthorizeRole("Seller")]
public sealed record GetSellerDashboardQuery(DateTimeOffset? From = null, DateTimeOffset? To = null)
    : IQuery<SellerDashboardResponse>, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Reports.ViewOwnShop;
}
