using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Messaging;
using NovaLive.Application.UseCases.Discounts;
using NovaLive.Contracts.V1.Discounts;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Discounts.Queries.GetShopDiscounts;

public sealed class GetShopDiscountsQueryHandler(IDiscountRepository discountRepository)
    : IQueryHandler<GetShopDiscountsQuery, IReadOnlyList<DiscountResponse>>
{
    public async Task<Result<IReadOnlyList<DiscountResponse>>> Handle(GetShopDiscountsQuery query, CancellationToken ct)
    {
        var discounts = await discountRepository.GetPublicShopDiscountsAsync(
            query.ShopId,
            DateTimeOffset.UtcNow,
            ct);

        var response = discounts.Select(d => d.ToResponse()).ToList();
        return response;
    }
}
