using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Messaging;
using NovaLive.Application.UseCases.Orders;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Returns;
using NovaLive.Domain.Common;
namespace NovaLive.Application.UseCases.Returns.Queries.GetSellerReturns;
public sealed class GetSellerReturnsQueryHandler(OrderShopAccess shops, ISubOrderRepository subs, IOrderReturnRepository returns, ReturnReader reader)
    : IQueryHandler<GetSellerReturnsQuery, PagedResult<OrderReturnResponse>>
{
    public async Task<Result<PagedResult<OrderReturnResponse>>> Handle(GetSellerReturnsQuery query, CancellationToken ct)
    {
        var shop = await shops.GetAsync(ct);
        if (shop.IsFailure) return ReturnErrors.NotFound;
        var ids = await subs.GetIdsByShopAsync(shop.Value, ct);
        if (query.SubOrderId is { } target && !ids.Contains(target)) return ReturnErrors.NotFound;
        var page = await returns.GetSellerPagedAsync(ids, query.Status == null ? null : Enum.Parse<ReturnStatus>(query.Status), query.Page, query.Size, ct, query.SubOrderId);
        return await reader.PageAsync(page.Items, page.Total, query.Page, query.Size, ct);
    }
}
