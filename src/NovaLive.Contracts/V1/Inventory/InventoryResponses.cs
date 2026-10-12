namespace NovaLive.Contracts.V1.Inventory;

public record InventoryResponse(
    Guid SkuId,
    string SkuCode,
    string SpuName,
    int QtyOnHand,
    int ReservedQty,
    int AvailableQty,
    int MinStock,
    DateTimeOffset LastUpdated);

public record InventoryHistoryResponse(
    Guid Id,
    Guid SkuId,
    string SkuCode,
    string SpuName,
    string ChangeType,
    int QtyBefore,
    int QtyChange,
    int ReservedBefore,
    int ReservedChange,
    int QtyAfter,
    string? RefType,
    Guid? RefId,
    string? Note,
    Guid? CreatedBy,
    DateTimeOffset CreatedAt);
