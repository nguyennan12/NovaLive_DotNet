using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Events;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Products;
using NovaLive.Domain.Common;
using NovaLive.Domain.System;

namespace NovaLive.Application.UseCases.Products.Commands.UpdateSkuPrice;

public sealed class UpdateSkuPriceCommandHandler(
    ISkuRepository skuRepository,
    IInventoryRepository inventoryRepository,
    IAppDbContext dbContext,
    ICurrentUser currentUser)
    : ICommandHandler<UpdateSkuPriceCommand, SkuResponse>
{
    public async Task<Result<SkuResponse>> Handle(UpdateSkuPriceCommand command, CancellationToken ct)
    {
        var shopId = currentUser.ShopId;
        if (!shopId.HasValue || shopId.Value == Guid.Empty)
        {
            return ProductErrors.UnauthorizedShop;
        }

        var sku = await skuRepository.GetByIdAndShopAsync(command.SkuId, shopId.Value, ct);

        if (sku is null)
        {
            return ProductErrors.SkuNotFound;
        }

        var req = command.Request;

        sku.UpdatePriceAndDetails(
            sellPrice: req.SellPrice,
            originalPrice: req.OriginalPrice,
            weightGram: req.WeightGram,
            isActive: req.IsActive);

        skuRepository.Update(sku);

        // Ghi OutboxMessage với typed Integration Event
        var integrationEvent = new ProductUpdatedIntegrationEvent(
            SpuId: sku.SpuId,
            ShopId: shopId.Value,
            Action: "SkuUpdated",
            SkuId: sku.Id);

        var outboxMessage = new OutboxMessage("ProductUpdatedEvent", JsonSerializer.Serialize(integrationEvent));
        await dbContext.OutboxMessages.AddAsync(outboxMessage, ct);

        var inventory = await inventoryRepository.GetBySkuIdAndShopAsync(sku.Id, shopId.Value, ct);

        var images = await dbContext.SkuImages
            .AsNoTracking()
            .Where(img => img.SkuId == sku.Id)
            .OrderBy(img => img.DisplayOrder)
            .Select(img => img.ImageUrl)
            .ToListAsync(ct);

        return sku.ToResponse(inventory, images);
    }
}
