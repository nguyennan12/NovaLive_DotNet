namespace NovaLive.Application.Abstractions.Persistence.Repositories;

public sealed record InventoryStockResult(
    Guid InventoryId,
    Guid SkuId,
    int QtyOnHand,
    int ReservedQty,
    int AvailableQty,
    int MinStock,
    int Version,
    bool AlreadyProcessed = false);
