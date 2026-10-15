using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Orders;
using NovaLive.Domain.Rbac;
namespace NovaLive.Application.UseCases.Orders.Queries.GetSellerOrders;
[RequireAuthenticated]
public sealed record GetSellerOrdersQuery(string? Status = null, DateTimeOffset? From = null, DateTimeOffset? To = null, int Page = 1, int Size = 20) : IQuery<PagedResult<SubOrderListItemResponse>>, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Orders.ViewOwnSell;
}
