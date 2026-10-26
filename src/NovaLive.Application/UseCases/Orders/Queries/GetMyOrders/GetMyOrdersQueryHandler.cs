using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Orders;
using NovaLive.Domain.Common;
using NovaLive.Domain.Orders;
namespace NovaLive.Application.UseCases.Orders.Queries.GetMyOrders;
public sealed class GetMyOrdersQueryHandler(IParentOrderRepository parents, ISubOrderRepository subs, ICurrentUser user)
    : IQueryHandler<GetMyOrdersQuery, PagedResult<ParentOrderListItemResponse>>
{
    public async Task<Result<PagedResult<ParentOrderListItemResponse>>> Handle(GetMyOrdersQuery query, CancellationToken ct)
    {
        if (!user.IsAuthenticated || user.UserId is not { } buyer || buyer == Guid.Empty) return CheckoutErrors.Unauthorized;
        var page = await parents.GetMinePagedAsync(buyer, query.Status, query.From, query.To, query.Page, query.Size, ct);
        var children = await subs.GetByParentIdsAsync(page.Items.Select(p => p.Id).ToArray(), false, ct);
        var childrenByParent = children.GroupBy(s => s.ParentOrderId).ToDictionary(g => g.Key, g => (IReadOnlyCollection<SubOrder>)g.ToArray());
        return new PagedResult<ParentOrderListItemResponse>(page.Items.Select(p => p.ToListItem(
            childrenByParent.GetValueOrDefault(p.Id, Array.Empty<SubOrder>()))).ToArray(), query.Page, query.Size, page.Total);
    }
}
