using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.FlashSales;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.FlashSales.Queries.GetActiveFlashSales;

public sealed class GetActiveFlashSalesQueryHandler(
    IFlashSaleRepository flashSaleRepository,
    ICacheService cacheService,
    IDateTimeProvider dateTimeProvider)
    : IQueryHandler<GetActiveFlashSalesQuery, IReadOnlyList<FlashSaleCampaignResponse>>
{
    private const string CacheKey = "flashsales:active";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(30);

    public async Task<Result<IReadOnlyList<FlashSaleCampaignResponse>>> Handle(
        GetActiveFlashSalesQuery query,
        CancellationToken ct)
    {
        var result = await cacheService.GetOrSetAsync<IReadOnlyList<FlashSaleCampaignResponse>>(
            CacheKey,
            async cancellationToken =>
            {
                var now = dateTimeProvider.UtcNow;
                var campaigns = await flashSaleRepository.GetActiveCampaignsAsync(now, cancellationToken);
                return campaigns.Select(c => c.ToResponse()).ToList();
            },
            CacheTtl,
            ct);

        return Result<IReadOnlyList<FlashSaleCampaignResponse>>.Success(result);
    }
}
