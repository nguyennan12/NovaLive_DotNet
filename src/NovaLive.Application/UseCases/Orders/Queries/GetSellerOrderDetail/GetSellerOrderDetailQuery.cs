using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Orders;
using NovaLive.Domain.Rbac;
namespace NovaLive.Application.UseCases.Orders.Queries.GetSellerOrderDetail;
[RequireAuthenticated]
public sealed record GetSellerOrderDetailQuery(Guid SubOrderId) : IQuery<SubOrderDetailResponse>, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Orders.ViewOwnSell;
}
