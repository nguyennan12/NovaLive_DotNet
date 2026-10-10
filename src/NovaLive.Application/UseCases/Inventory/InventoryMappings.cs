using NovaLive.Contracts.V1.Products;
using NovaLive.Domain.Inventory;
using NovaLive.Domain.Products;

namespace NovaLive.Application.UseCases.Inventory;

public static class InventoryMappings
{
    public static InventoryResponse ToResponse(
        this NovaLive.Domain.Inventory.Inventory inventory,
        string skuCode,
        string spuName)
    {
        return new InventoryResponse(
            SkuId: inventory.SkuId,
            SkuCode: skuCode,
            SpuName: spuName,
            QtyOnHand: inventory.QtyOnHand,
            ReservedQty: inventory.ReservedQty,
            AvailableQty: inventory.AvailableQty,
            MinStock: inventory.MinStock,
            LastUpdated: inventory.LastUpdated);
    }

    public static InventoryHistoryResponse ToHistoryResponse(
        this InventoryHistory history,
        string skuCode,
        string spuName)
    {
        return new InventoryHistoryResponse(
            Id: history.Id,
            SkuId: history.SkuId,
            SkuCode: skuCode,
            SpuName: spuName,
            ChangeType: history.ChangeType.ToString(),
            QtyBefore: history.QtyBefore,
            QtyChange: history.QtyChange,
            ReservedBefore: history.ReservedBefore,
            ReservedChange: history.ReservedChange,
            QtyAfter: history.QtyAfter,
            RefType: history.RefType,
            RefId: history.RefId,
            Note: history.Note,
            CreatedBy: history.CreatedBy,
            CreatedAt: history.CreatedAt);
    }
}
