using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.FlashSales;
using NovaLive.Domain.Common;
using NovaLive.Domain.FlashSales;

namespace NovaLive.Application.UseCases.FlashSales.Commands.RejectFlashSaleItem;

public sealed class RejectFlashSaleItemCommandHandler(
    IFlashSaleRepository flashSaleRepository,
    ISkuRepository skuRepository,
    ISpuRepository spuRepository)
    : ICommandHandler<RejectFlashSaleItemCommand, FlashSaleItemResponse>
{
    public async Task<Result<FlashSaleItemResponse>> Handle(
        RejectFlashSaleItemCommand command,
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

        item.Reject(command.Request.Reason);

        await flashSaleRepository.SaveChangesAsync(ct);

        var sku = await skuRepository.GetByIdAsync(item.SkuId, ct);
        var spu = sku != null ? await spuRepository.GetByIdAsync(sku.SpuId, ct) : null;

        return item.ToResponse(sku, spu);
    }
}
