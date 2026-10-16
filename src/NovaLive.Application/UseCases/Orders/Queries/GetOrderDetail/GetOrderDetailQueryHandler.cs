using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Orders;
using NovaLive.Domain.Common;
namespace NovaLive.Application.UseCases.Orders.Queries.GetOrderDetail;
public sealed class GetOrderDetailQueryHandler(OrderDetailsReader reader, ICurrentUser user)
    : IQueryHandler<GetOrderDetailQuery, ParentOrderDetailResponse>
{
    public Task<Result<ParentOrderDetailResponse>> Handle(GetOrderDetailQuery query, CancellationToken ct) =>
        !user.IsAuthenticated || user.UserId is not { } buyer || buyer == Guid.Empty
            ? Task.FromResult<Result<ParentOrderDetailResponse>>(CheckoutErrors.Unauthorized)
            : reader.GetBuyerAsync(query.OrderId, buyer, ct);
}
