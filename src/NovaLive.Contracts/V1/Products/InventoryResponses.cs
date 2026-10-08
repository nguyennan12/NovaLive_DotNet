namespace NovaLive.Contracts.V1.Products;

public record InventoryResponse(
    Guid SkuId,
    string SkuCode,
    string SpuName,
    int QtyOnHand,
    int ReservedQty,
    int AvailableQty,
    DateTime LastUpdated);

public record InventoryHistoryResponse(
    Guid Id,
    Guid SkuId,
    int QtyChange,
    string ChangeType,
    int QtyAfter,
    string? Note,
    DateTime CreatedAt);
