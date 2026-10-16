using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Domain.Rbac;

namespace NovaLive.Application.UseCases.Shipping.Queries.GetShippingLabel;

[RequireAuthenticated]
public sealed record GetShippingLabelQuery(Guid SubOrderId) : IQuery<byte[]>, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Shipping.PrintOwnLabel;
}
