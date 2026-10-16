using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Orders;
using NovaLive.Domain.Common;
namespace NovaLive.Application.UseCases.Orders.Queries.GetSellerOrders;
public sealed class GetSellerOrdersQueryHandler(ISubOrderRepository subs, OrderShopAccess shopAccess)
    : IQueryHandler<GetSellerOrdersQuery, PagedResult<SubOrderListItemResponse>>
{
    public async Task<Result<PagedResult<SubOrderListItemResponse>>> Handle(GetSellerOrdersQuery query, CancellationToken ct)
    {
        var context = await shopAccess.GetAsync(ct);
        if (context.IsFailure) return context.Error;
        var shopId = context.Value;
        SubOrderStatus? status = query.Status is null ? null : Enum.Parse<SubOrderStatus>(query.Status, true);
        var page = await subs.GetSellerPagedAsync(shopId, status, query.From, query.To, query.Page, query.Size, ct);
        return new PagedResult<SubOrderListItemResponse>(page.Items.Select(s => s.ToListItem()).ToArray(), query.Page, query.Size, page.Total);
    }
}
