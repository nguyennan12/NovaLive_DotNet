using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Orders;
using NovaLive.Domain.Rbac;
namespace NovaLive.Application.UseCases.Orders.Queries.GetOrderDetail;
[RequireAuthenticated]
public sealed record GetOrderDetailQuery(Guid OrderId) : IQuery<ParentOrderDetailResponse>, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Orders.ViewOwnBuy;
}
