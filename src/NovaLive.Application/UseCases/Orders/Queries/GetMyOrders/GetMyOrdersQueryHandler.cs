using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Orders;
using NovaLive.Domain.Common;
namespace NovaLive.Application.UseCases.Orders.Queries.GetMyOrders;
public sealed class GetMyOrdersQueryHandler(IParentOrderRepository parents, ICurrentUser user)
    : IQueryHandler<GetMyOrdersQuery, PagedResult<ParentOrderListItemResponse>>
{
    public async Task<Result<PagedResult<ParentOrderListItemResponse>>> Handle(GetMyOrdersQuery query, CancellationToken ct)
    {
        if (!user.IsAuthenticated || user.UserId is not { } buyer || buyer == Guid.Empty) return CheckoutErrors.Unauthorized;
        ParentOrderPaymentStatus? status = query.Status is null ? null : Enum.Parse<ParentOrderPaymentStatus>(query.Status, true);
        var page = await parents.GetMinePagedAsync(buyer, status, query.From, query.To, query.Page, query.Size, ct);
        return new PagedResult<ParentOrderListItemResponse>(page.Items.Select(p => p.ToListItem()).ToArray(), query.Page, query.Size, page.Total);
    }
}
