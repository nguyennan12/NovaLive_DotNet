using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Messaging;
using NovaLive.Application.UseCases.Discounts;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Discounts;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Discounts.Queries.GetPlatformDiscountsAdmin;

public sealed class GetPlatformDiscountsAdminQueryHandler(
    IDiscountRepository discountRepository)
    : IQueryHandler<GetPlatformDiscountsAdminQuery, PagedResult<DiscountResponse>>
{
    public async Task<Result<PagedResult<DiscountResponse>>> Handle(GetPlatformDiscountsAdminQuery query, CancellationToken ct)
    {
        var pagedRequest = new PagedRequest(query.Page, query.Size);

        var (items, total) = await discountRepository.GetPlatformDiscountsAdminAsync(
            query.Status,
            pagedRequest.Skip,
            pagedRequest.Take,
            DateTimeOffset.UtcNow,
            ct);

        var dtos = items.Select(d => d.ToResponse()).ToList();

        return PagedResult<DiscountResponse>.Create(
            dtos,
            pagedRequest.NormalizedPage,
            pagedRequest.NormalizedSize,
            total);
    }
}
