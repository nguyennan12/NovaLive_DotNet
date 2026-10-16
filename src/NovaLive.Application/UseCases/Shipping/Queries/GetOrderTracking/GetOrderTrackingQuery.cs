using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Shipping;
using NovaLive.Domain.Rbac;

namespace NovaLive.Application.UseCases.Shipping.Queries.GetOrderTracking;

[RequireAuthenticated]
public sealed record GetOrderTrackingQuery(Guid OrderId, bool IsSeller = false)
    : IQuery<IReadOnlyList<TrackingResponse>>, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Shipping.TrackOrder;
}
