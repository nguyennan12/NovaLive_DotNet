using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.FlashSales;
using NovaLive.Domain.Common;
using NovaLive.Domain.Products;

namespace NovaLive.Application.UseCases.FlashSales.Queries.GetFlashSaleProducts;

public sealed class GetFlashSaleProductsQueryHandler(
    IFlashSaleRepository flashSaleRepository,
    ISkuRepository skuRepository,
    ISpuRepository spuRepository,
    ICacheService cacheService)
    : IQueryHandler<GetFlashSaleProductsQuery, PagedResult<FlashSaleItemResponse>>
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(15);

    public async Task<Result<PagedResult<FlashSaleItemResponse>>> Handle(
        GetFlashSaleProductsQuery query,
        CancellationToken ct)
    {
        var campaign = await flashSaleRepository.GetCampaignByIdAsync(query.CampaignId, ct);
        if (campaign is null)
        {
            return FlashSalesErrors.CampaignNotFound;
        }

        var cacheKey = $"flashsales:campaign:{query.CampaignId}:products:p{query.Page}:s{query.Size}";

        var result = await cacheService.GetOrSetAsync(
            cacheKey,
            async cancellationToken =>
            {
                var skip = (query.Page - 1) * query.Size;
                var take = query.Size;

                var (items, totalCount) = await flashSaleRepository.GetApprovedItemsByCampaignAsync(
                    query.CampaignId,
                    skip,
                    take,
                    cancellationToken);

                if (items.Count == 0)
                {
                    return PagedResult<FlashSaleItemResponse>.Create([], query.Page, query.Size, totalCount);
                }

                var skuIds = items.Select(i => i.SkuId).Distinct().ToList();
                var skus = await skuRepository.GetByIdsAsync(skuIds, cancellationToken);
                var skuMap = skus.ToDictionary(s => s.Id);

                var spuIds = skus.Select(s => s.SpuId).Distinct().ToList();
                var spus = await spuRepository.GetByIdsAsync(spuIds, cancellationToken);
                var spuMap = spus.ToDictionary(s => s.Id);

                var responses = items.Select(item =>
                {
                    skuMap.TryGetValue(item.SkuId, out var sku);
                    Spu? spu = null;
                    if (sku != null)
                    {
                        spuMap.TryGetValue(sku.SpuId, out spu);
                    }

                    return item.ToResponse(sku, spu);
                }).ToList();

                return PagedResult<FlashSaleItemResponse>.Create(responses, query.Page, query.Size, totalCount);
            },
            CacheTtl,
            ct);

        return Result<PagedResult<FlashSaleItemResponse>>.Success(result);
    }
}
