using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Returns;
using NovaLive.Domain.Common;
namespace NovaLive.Application.UseCases.Returns.Queries.GetReturnDetail;
public sealed class GetReturnDetailQueryHandler(IOrderReturnRepository returns, ICurrentUser user, ReturnReader reader)
    : IQueryHandler<GetReturnDetailQuery, OrderReturnResponse>
{
    public async Task<Result<OrderReturnResponse>> Handle(GetReturnDetailQuery query, CancellationToken ct)
    {
        if (!user.IsAuthenticated || user.UserId is not { } buyer) return ReturnErrors.Unauthorized;
        var request = await returns.FindAsync(query.Id, ct);
        if (request is null || request.BuyerId != buyer) return ReturnErrors.NotFound;
        var detail = await reader.DetailAsync(request, ct);
        if (detail is null) return ReturnErrors.NotFound;
        return detail;
    }
}
