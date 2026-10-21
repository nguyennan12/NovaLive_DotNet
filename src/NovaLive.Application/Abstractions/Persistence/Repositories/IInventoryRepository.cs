using NovaLive.Domain.Common;
using NovaLive.Domain.Inventory;
using NovaLive.Domain.Products;

namespace NovaLive.Application.Abstractions.Persistence.Repositories;

public interface IInventoryRepository
{
    // Caller locks SKU rows in ascending order and writes the ledger in the same transaction.
    Task<Result<InventoryStockResult>> ConfirmReservedSaleAsync(Guid skuId, Guid shopId, int quantity,
        DateTimeOffset now, CancellationToken ct);
    Task<Result<InventoryStockResult>> ReleaseReservedAsync(Guid skuId, Guid shopId, int quantity,
        DateTimeOffset now, CancellationToken ct);

    Task<List<Inventory>> GetByIdsForUpdateAsync(IEnumerable<Guid> skuIds, CancellationToken ct);

    // Queries & Read
    Task<Inventory?> GetBySkuIdAndShopAsync(Guid skuId, Guid shopId, CancellationToken ct = default);
    Task<Dictionary<Guid, Inventory>> GetBySkuIdsAsync(IEnumerable<Guid> skuIds, CancellationToken ct = default);
    Task<(List<(Inventory Inventory, Sku Sku, Spu Spu)> Items, long TotalCount)> GetSellerInventoryPagedAsync(
        Guid shopId,
        bool? lowStock,
        string? keyword,
        int page,
        int size,
        CancellationToken ct = default);

    Task<(List<(InventoryHistory History, string SkuCode, string SpuName)> Items, long TotalCount)> GetHistoriesPagedAsync(
        Guid shopId,
        Guid? skuId,
        string? changeType,
        DateTimeOffset? from,
        DateTimeOffset? to,
        int page,
        int size,
        CancellationToken ct = default);

    // Basic CRUD
    Task AddAsync(Inventory inventory, CancellationToken ct = default);
    Task AddHistoryAsync(InventoryHistory history, CancellationToken ct = default);
    void Update(Inventory inventory);

    // Atomic Stock Operations (State Machine + Idempotent Ledger)
    Task<Result<InventoryStockResult>> ReserveStockAsync(
        Guid skuId,
        Guid shopId,
        int qty,
        string refType,
        Guid refId,
        Guid operationId,
        Guid? userId,
        CancellationToken ct = default);

    Task<Result<InventoryStockResult>> ReleaseStockAsync(
        Guid skuId,
        Guid shopId,
        int qty,
        string refType,
        Guid refId,
        Guid operationId,
        Guid? userId,
        CancellationToken ct = default);

    Task<Result<InventoryStockResult>> ConfirmSaleAsync(
        Guid skuId,
        Guid shopId,
        int qty,
        string refType,
        Guid refId,
        Guid operationId,
        Guid? userId,
        CancellationToken ct = default);

    Task<Result<InventoryStockResult>> ReturnStockAsync(
        Guid skuId,
        Guid shopId,
        int qty,
        string refType,
        Guid refId,
        Guid operationId,
        Guid? userId,
        CancellationToken ct = default);

    Task<Result<InventoryStockResult>> CancelSaleAsync(
        Guid skuId,
        Guid shopId,
        int qty,
        string refType,
        Guid refId,
        Guid operationId,
        Guid? userId,
        CancellationToken ct = default);

    Task<Result<InventoryStockResult>> AdjustStockAsync(
        Guid skuId,
        Guid shopId,
        int qtyChange,
        InventoryChangeType changeType,
        string? note,
        Guid operationId,
        Guid? userId,
        CancellationToken ct = default);
}
