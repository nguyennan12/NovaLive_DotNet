using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.FlashSales;
using NovaLive.Domain.Common;
using NovaLive.Domain.FlashSales;

namespace NovaLive.Application.UseCases.FlashSales.Commands.RegisterSkuForFlashSale;

public sealed class RegisterSkuForFlashSaleCommandHandler(
    IFlashSaleRepository flashSaleRepository,
    ISkuRepository skuRepository,
    ISpuRepository spuRepository,
    IInventoryRepository inventoryRepository,
    ICurrentUser currentUser,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<RegisterSkuForFlashSaleCommand, FlashSaleItemResponse>
{
    public async Task<Result<FlashSaleItemResponse>> Handle(
        RegisterSkuForFlashSaleCommand command,
        CancellationToken ct)
    {
        var shopId = currentUser.ShopId;
        if (!shopId.HasValue || shopId.Value == Guid.Empty)
        {
            return FlashSalesErrors.UnauthorizedShop;
        }

        var campaign = await flashSaleRepository.GetCampaignByIdAsync(command.CampaignId, ct);
        if (campaign is null)
        {
            return FlashSalesErrors.CampaignNotFound;
        }

        var now = dateTimeProvider.UtcNow;
        if (campaign.Status == FlashSaleCampaignStatus.Cancelled)
        {
            return FlashSalesErrors.CampaignCancelled;
        }

        if (campaign.Status != FlashSaleCampaignStatus.Scheduled || now >= campaign.StartAt)
        {
            return FlashSalesErrors.CampaignRegistrationClosed;
        }

        var req = command.Request;

        var sku = await skuRepository.GetByIdAsync(req.SkuId, ct);
        if (sku is null || sku.DeletedAt != null)
        {
            return FlashSalesErrors.SkuNotFound;
        }

        if (sku.ShopId != shopId.Value)
        {
            return FlashSalesErrors.SkuNotBelongToShop;
        }

        if (!sku.IsActive)
        {
            return FlashSalesErrors.SkuNotActive;
        }

        var maxAllowedPrice = sku.SellPrice * 0.90m;
        if (req.FlashPrice > maxAllowedPrice)
        {
            return FlashSalesErrors.FlashPriceMustBe10PercentLower;
        }

        var inventory = await inventoryRepository.GetBySkuIdAndShopAsync(sku.Id, shopId.Value, ct);
        if (inventory is null || (inventory.QtyOnHand - inventory.ReservedQty) < req.Quantity)
        {
            return FlashSalesErrors.InsufficientInventoryStock;
        }

        if (await flashSaleRepository.HasOverlappingFlashSaleAsync(sku.Id, campaign.StartAt, campaign.EndAt, campaign.Id, ct))
        {
            return FlashSalesErrors.SkuOverlapsWithAnotherCampaign;
        }

        if (await flashSaleRepository.ExistsItemAsync(campaign.Id, sku.Id, ct))
        {
            return FlashSalesErrors.SkuAlreadyRegisteredInCampaign;
        }

        var item = new FlashSaleItem(
            campaignId: campaign.Id,
            skuId: sku.Id,
            shopId: shopId.Value,
            flashPrice: req.FlashPrice,
            quantity: req.Quantity,
            perUserLimit: req.PerUserLimit);

        await flashSaleRepository.AddItemAsync(item, ct);

        try
        {
            await flashSaleRepository.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            return FlashSalesErrors.SkuAlreadyRegisteredInCampaign;
        }

        var spu = await spuRepository.GetByIdAsync(sku.SpuId, ct);

        return item.ToResponse(sku, spu);
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        return ex.InnerException?.Message.Contains("23505") == true ||
               ex.InnerException?.Message.Contains("ix_flash_sale_items_campaign_id_sku_id") == true;
    }
}
