using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Messaging;
using NovaLive.Application.UseCases.Discounts;
using NovaLive.Contracts.V1.Discounts;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Discounts.Queries.GetPlatformDiscounts;

public sealed class GetPlatformDiscountsQueryHandler(IDiscountRepository discountRepository)
    : IQueryHandler<GetPlatformDiscountsQuery, IReadOnlyList<DiscountResponse>>
{
    public async Task<Result<IReadOnlyList<DiscountResponse>>> Handle(GetPlatformDiscountsQuery query, CancellationToken ct)
    {
        var discounts = await discountRepository.GetPublicPlatformDiscountsAsync(
            DateTimeOffset.UtcNow,
            ct);

        var response = discounts.Select(d => d.ToResponse()).ToList();
        return response;
    }
}
