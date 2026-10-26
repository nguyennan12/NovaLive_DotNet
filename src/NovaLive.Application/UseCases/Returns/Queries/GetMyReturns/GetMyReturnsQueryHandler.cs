using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Returns;
using NovaLive.Domain.Common;
namespace NovaLive.Application.UseCases.Returns.Queries.GetMyReturns;
public sealed class GetMyReturnsQueryHandler(IOrderReturnRepository returns, ICurrentUser user, ReturnReader reader)
    : IQueryHandler<GetMyReturnsQuery, PagedResult<OrderReturnResponse>>
{
    public async Task<Result<PagedResult<OrderReturnResponse>>> Handle(GetMyReturnsQuery query, CancellationToken ct)
    {
        if (!user.IsAuthenticated || user.UserId is not { } buyer) return ReturnErrors.Unauthorized;
        var page = await returns.GetBuyerPagedAsync(buyer, query.Status == null ? null : Enum.Parse<ReturnStatus>(query.Status), query.Page, query.Size, ct);
        return await reader.PageAsync(page.Items, page.Total, query.Page, query.Size, ct);
    }
}
