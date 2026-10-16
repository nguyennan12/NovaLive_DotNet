using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Orders;
using NovaLive.Domain.Common;
namespace NovaLive.Application.UseCases.Orders.Queries.GetSellerOrderDetail;
public sealed class GetSellerOrderDetailQueryHandler(OrderDetailsReader reader, OrderShopAccess shopAccess)
    : IQueryHandler<GetSellerOrderDetailQuery, SubOrderDetailResponse>
{
    public async Task<Result<SubOrderDetailResponse>> Handle(GetSellerOrderDetailQuery query, CancellationToken ct)
    {
        var context = await shopAccess.GetAsync(ct);
        if (context.IsFailure) return context.Error;
        return await reader.GetSellerAsync(query.SubOrderId, context.Value, ct);
    }
}
