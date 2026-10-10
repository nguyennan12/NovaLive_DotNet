using System.Text.Json;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Events;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Inventory;
using NovaLive.Domain.Common;
using NovaLive.Domain.Inventory;
using NovaLive.Domain.System;

namespace NovaLive.Application.UseCases.Inventory.Commands.AdjustInventory;

public sealed class AdjustInventoryCommandHandler(
    IInventoryRepository inventoryRepository,
    ISkuRepository skuRepository,
    ISpuRepository spuRepository,
    IAppDbContext dbContext,
    ICurrentUser currentUser)
    : ICommandHandler<AdjustInventoryCommand, InventoryResponse>
{
    public async Task<Result<InventoryResponse>> Handle(AdjustInventoryCommand command, CancellationToken ct)
    {
        var shopId = currentUser.ShopId;
        if (!shopId.HasValue || shopId.Value == Guid.Empty)
        {
            return InventoryErrors.UnauthorizedShop;
        }

        var req = command.Request;

        if (!Enum.TryParse<InventoryChangeType>(req.ChangeType, ignoreCase: true, out var changeType))
        {
            return InventoryErrors.InvalidChangeType;
        }

        var sku = await skuRepository.GetByIdAndShopAsync(req.SkuId, shopId.Value, ct);

        if (sku is null)
        {
            return InventoryErrors.SkuNotFound;
        }

        var spu = await spuRepository.GetByIdAsync(sku.SpuId, ct);

        var operationId = req.OperationId ?? Guid.NewGuid();

        var stockResult = await inventoryRepository.AdjustStockAsync(
            skuId: req.SkuId,
            shopId: shopId.Value,
            qtyChange: req.QtyChange,
            changeType: changeType,
            note: req.Note,
            operationId: operationId,
            userId: currentUser.UserId,
            ct: ct);

        if (stockResult.IsFailure)
        {
            return stockResult.Error;
        }

        var res = stockResult.Value!;

        // Ghi Outbox Message để đồng bộ cache/search
        var integrationEvent = new ProductUpdatedIntegrationEvent(
            SpuId: sku.SpuId,
            ShopId: shopId.Value,
            Action: "InventoryAdjusted",
            SkuId: sku.Id);

        var outboxMessage = new OutboxMessage("ProductUpdatedEvent", JsonSerializer.Serialize(integrationEvent));
        await dbContext.OutboxMessages.AddAsync(outboxMessage, ct);

        return new InventoryResponse(
            SkuId: res.SkuId,
            SkuCode: sku.SkuCode,
            SpuName: spu?.Name ?? "Sản phẩm",
            QtyOnHand: res.QtyOnHand,
            ReservedQty: res.ReservedQty,
            AvailableQty: res.AvailableQty,
            MinStock: res.MinStock,
            LastUpdated: DateTimeOffset.UtcNow);
    }
}
