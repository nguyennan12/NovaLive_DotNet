using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Dashboards;
using NovaLive.Domain.Rbac;

namespace NovaLive.Application.UseCases.Dashboards.Admin.GetAdminDashboard;

public sealed record GetAdminDashboardQuery(DateTimeOffset? From = null, DateTimeOffset? To = null)
    : IQuery<AdminDashboardResponse>, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Reports.ViewAllPlatform;
}
