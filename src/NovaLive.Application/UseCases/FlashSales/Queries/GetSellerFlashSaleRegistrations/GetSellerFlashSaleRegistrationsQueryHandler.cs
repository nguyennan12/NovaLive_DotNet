using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.FlashSales;
using NovaLive.Domain.Common;
using NovaLive.Domain.FlashSales;
using NovaLive.Domain.Products;

namespace NovaLive.Application.UseCases.FlashSales.Queries.GetSellerFlashSaleRegistrations;

public sealed class GetSellerFlashSaleRegistrationsQueryHandler(
    IFlashSaleRepository flashSaleRepository,
    ISkuRepository skuRepository,
    ISpuRepository spuRepository,
    ICurrentUser currentUser)
    : IQueryHandler<GetSellerFlashSaleRegistrationsQuery, PagedResult<FlashSaleItemResponse>>
{
    public async Task<Result<PagedResult<FlashSaleItemResponse>>> Handle(
        GetSellerFlashSaleRegistrationsQuery query,
        CancellationToken ct)
    {
        var shopId = currentUser.ShopId;
        if (!shopId.HasValue || shopId.Value == Guid.Empty)
        {
            return FlashSalesErrors.UnauthorizedShop;
        }

        FlashSaleItemStatus? status = null;
        if (!string.IsNullOrWhiteSpace(query.Status) &&
            Enum.TryParse<FlashSaleItemStatus>(query.Status, true, out var parsedStatus))
        {
            status = parsedStatus;
        }

        var skip = (query.Page - 1) * query.Size;
        var take = query.Size;

        var (items, totalCount) = await flashSaleRepository.GetSellerRegistrationsAsync(
            shopId.Value,
            query.CampaignId,
            status,
            skip,
            take,
            ct);

        if (items.Count == 0)
        {
            return PagedResult<FlashSaleItemResponse>.Create([], query.Page, query.Size, totalCount);
        }

        var skuIds = items.Select(i => i.SkuId).Distinct().ToList();
        var skus = await skuRepository.GetByIdsAsync(skuIds, ct);
        var skuMap = skus.ToDictionary(s => s.Id);

        var spuIds = skus.Select(s => s.SpuId).Distinct().ToList();
        var spus = await spuRepository.GetByIdsAsync(spuIds, ct);
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
    }
}
