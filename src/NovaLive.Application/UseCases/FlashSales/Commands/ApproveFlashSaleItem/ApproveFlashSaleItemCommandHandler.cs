using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.FlashSales;
using NovaLive.Domain.Common;
using NovaLive.Domain.FlashSales;

namespace NovaLive.Application.UseCases.FlashSales.Commands.ApproveFlashSaleItem;

public sealed class ApproveFlashSaleItemCommandHandler(
    IFlashSaleRepository flashSaleRepository,
    ISkuRepository skuRepository,
    ISpuRepository spuRepository,
    ICacheService cacheService)
    : ICommandHandler<ApproveFlashSaleItemCommand, FlashSaleItemResponse>
{
    public async Task<Result<FlashSaleItemResponse>> Handle(
        ApproveFlashSaleItemCommand command,
        CancellationToken ct)
    {
        var item = await flashSaleRepository.GetItemByIdAsync(command.ItemId, ct);
        if (item is null)
        {
            return FlashSalesErrors.ItemNotFound;
        }

        if (item.Status != FlashSaleItemStatus.Pending)
        {
            return FlashSalesErrors.ItemNotPending;
        }

        item.Approve();

        await flashSaleRepository.SaveChangesAsync(ct);

        await cacheService.RemoveAsync("flashsales:active", ct);
        await cacheService.RemoveByPrefixAsync($"flashsales:campaign:{item.CampaignId}", ct);

        var sku = await skuRepository.GetByIdAsync(item.SkuId, ct);
        var spu = sku != null ? await spuRepository.GetByIdAsync(sku.SpuId, ct) : null;

        return item.ToResponse(sku, spu);
    }
}
