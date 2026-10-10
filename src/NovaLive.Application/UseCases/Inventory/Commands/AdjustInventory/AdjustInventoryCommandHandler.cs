using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Common.Events;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Products;
using NovaLive.Domain.Common;
using NovaLive.Domain.Inventory;
using NovaLive.Domain.System;

namespace NovaLive.Application.UseCases.Inventory.Commands.AdjustInventory;

public sealed class AdjustInventoryCommandHandler(
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

        // 1. Lấy thông tin Tồn kho của SKU thuộc Shop
        var inventory = await dbContext.Inventories
            .FirstOrDefaultAsync(i => i.SkuId == req.SkuId && i.ShopId == shopId.Value, ct);

        if (inventory is null)
        {
            return InventoryErrors.NotFound;
        }

        var sku = await dbContext.Skus
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == req.SkuId && s.ShopId == shopId.Value && s.DeletedAt == null, ct);

        if (sku is null)
        {
            return InventoryErrors.SkuNotFound;
        }

        var spu = await dbContext.Spus
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == sku.SpuId, ct);

        // 2. Ràng buộc cốt lõi (FR-CAT-008): qty_on_hand + qtyChange không được < reserved_qty
        if (inventory.QtyOnHand + req.QtyChange < inventory.ReservedQty)
        {
            return InventoryErrors.InsufficientStock;
        }

        // 3. Parse loại điều chỉnh kho
        if (!Enum.TryParse<InventoryChangeType>(req.ChangeType, ignoreCase: true, out var changeType))
        {
            return InventoryErrors.InvalidChangeType;
        }

        var qtyBefore = inventory.QtyOnHand;

        // 4. Cập nhật tồn kho vật lý
        inventory.AdjustOnHand(req.QtyChange);

        // 5. Ghi nhận sổ cái bất biến (Append-only Ledger - FR-CAT-006, NFR-REL-006)
        var history = new InventoryHistory(
            inventoryId: inventory.Id,
            skuId: sku.Id,
            changeType: changeType,
            qtyBefore: qtyBefore,
            qtyChange: req.QtyChange,
            reservedBefore: inventory.ReservedQty,
            reservedChange: 0,
            qtyAfter: inventory.QtyOnHand,
            refType: "ManualAdjustment",
            refId: null,
            note: req.Note,
            createdBy: currentUser.UserId);

        await dbContext.InventoryHistories.AddAsync(history, ct);

        // 6. Ghi Outbox Message để đồng bộ cache/search
        var integrationEvent = new ProductUpdatedIntegrationEvent(
            SpuId: sku.SpuId,
            ShopId: shopId.Value,
            Action: "InventoryAdjusted",
            SkuId: sku.Id);

        var outboxMessage = new OutboxMessage("ProductUpdatedEvent", JsonSerializer.Serialize(integrationEvent));
        await dbContext.OutboxMessages.AddAsync(outboxMessage, ct);

        return inventory.ToResponse(sku.SkuCode, spu?.Name ?? "Sản phẩm");
    }
}
