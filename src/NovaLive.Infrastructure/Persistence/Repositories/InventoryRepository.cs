using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.UseCases.Inventory;
using NovaLive.Domain.Common;
using NovaLive.Domain.Inventory;
using NovaLive.Domain.Products;

namespace NovaLive.Infrastructure.Persistence.Repositories;

public sealed class InventoryRepository(IAppDbContext dbContext) : IInventoryRepository
{
    public async Task<Inventory?> GetBySkuIdAndShopAsync(Guid skuId, Guid shopId, CancellationToken ct = default)
    {
        return await dbContext.Inventories
            .FirstOrDefaultAsync(i => i.SkuId == skuId && i.ShopId == shopId, ct);
    }

    public async Task<Dictionary<Guid, Inventory>> GetBySkuIdsAsync(IEnumerable<Guid> skuIds, CancellationToken ct = default)
    {
        var idList = skuIds.Distinct().ToList();
        return await dbContext.Inventories
            .AsNoTracking()
            .Where(i => idList.Contains(i.SkuId))
            .ToDictionaryAsync(i => i.SkuId, ct);
    }

    public async Task<(List<(Inventory Inventory, Sku Sku, Spu Spu)> Items, long TotalCount)> GetSellerInventoryPagedAsync(
        Guid shopId,
        bool? lowStock,
        string? keyword,
        int page,
        int size,
        CancellationToken ct = default)
    {
        var baseQuery = from inventory in dbContext.Inventories.AsNoTracking()
                        join sku in dbContext.Skus.AsNoTracking() on inventory.SkuId equals sku.Id
                        join spu in dbContext.Spus.AsNoTracking() on sku.SpuId equals spu.Id
                        where inventory.ShopId == shopId
                            && sku.DeletedAt == null
                            && spu.DeletedAt == null
                        select new { Inventory = inventory, Sku = sku, Spu = spu };

        if (lowStock == true)
        {
            baseQuery = baseQuery.Where(x => (x.Inventory.QtyOnHand - x.Inventory.ReservedQty) <= x.Inventory.MinStock);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var k = keyword.Trim().ToLower();
            baseQuery = baseQuery.Where(x => x.Sku.SkuCode.ToLower().Contains(k) || x.Spu.Name.ToLower().Contains(k));
        }

        var total = await baseQuery.LongCountAsync(ct);

        var pagedItems = await baseQuery
            .OrderByDescending(x => x.Inventory.LastUpdated)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        var items = pagedItems.Select(x => (x.Inventory, x.Sku, x.Spu)).ToList();
        return (items, total);
    }

    public async Task<(List<(InventoryHistory History, string SkuCode, string SpuName)> Items, long TotalCount)> GetHistoriesPagedAsync(
        Guid shopId,
        Guid? skuId,
        string? changeType,
        DateTimeOffset? from,
        DateTimeOffset? to,
        int page,
        int size,
        CancellationToken ct = default)
    {
        var baseQuery = from history in dbContext.InventoryHistories.AsNoTracking()
                        join sku in dbContext.Skus.AsNoTracking() on history.SkuId equals sku.Id
                        join spu in dbContext.Spus.AsNoTracking() on sku.SpuId equals spu.Id
                        where sku.ShopId == shopId
                        select new { History = history, SkuCode = sku.SkuCode, SpuName = spu.Name };

        if (skuId.HasValue && skuId.Value != Guid.Empty)
        {
            baseQuery = baseQuery.Where(x => x.History.SkuId == skuId.Value);
        }

        if (!string.IsNullOrWhiteSpace(changeType) && Enum.TryParse<InventoryChangeType>(changeType, ignoreCase: true, out var parsedType))
        {
            baseQuery = baseQuery.Where(x => x.History.ChangeType == parsedType);
        }

        if (from.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.History.CreatedAt >= from.Value);
        }

        if (to.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.History.CreatedAt <= to.Value);
        }

        var total = await baseQuery.LongCountAsync(ct);

        var pagedItems = await baseQuery
            .OrderByDescending(x => x.History.CreatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        var items = pagedItems.Select(x => (x.History, x.SkuCode, x.SpuName)).ToList();
        return (items, total);
    }

    public async Task AddAsync(Inventory inventory, CancellationToken ct = default)
    {
        await dbContext.Inventories.AddAsync(inventory, ct);
    }

    public async Task AddHistoryAsync(InventoryHistory history, CancellationToken ct = default)
    {
        await dbContext.InventoryHistories.AddAsync(history, ct);
    }

    public void Update(Inventory inventory)
    {
        dbContext.Inventories.Update(inventory);
    }

    public Task<Result<InventoryStockResult>> ReserveStockAsync(
        Guid skuId, Guid shopId, int qty, string refType, Guid refId, Guid operationId, Guid? userId, CancellationToken ct = default)
    {
        return qty <= 0
            ? Task.FromResult<Result<InventoryStockResult>>(InventoryErrors.InvalidQtyChange)
            : ExecuteStockTransitionAsync(
                skuId, shopId, operationId, userId, refType, refId, $"Giữ chỗ đơn hàng {refId}",
                InventoryChangeType.ReserveAdd, qtyChange: 0, reservedChange: qty,
                validatePrecondition: inv => inv.AvailableQty < qty ? InventoryErrors.InsufficientStock : null,
                applyTransition: inv => inv.Reserve(qty),
                ct: ct);
    }

    public Task<Result<InventoryStockResult>> ReleaseStockAsync(
        Guid skuId, Guid shopId, int qty, string refType, Guid refId, Guid operationId, Guid? userId, CancellationToken ct = default)
    {
        return qty <= 0
            ? Task.FromResult<Result<InventoryStockResult>>(InventoryErrors.InvalidQtyChange)
            : ExecuteStockTransitionAsync(
                skuId, shopId, operationId, userId, refType, refId, $"Giải phóng giữ chỗ đơn hàng {refId}",
                InventoryChangeType.ReserveRelease, qtyChange: 0, reservedChange: -qty,
                validatePrecondition: inv => inv.ReservedQty < qty ? InventoryErrors.InsufficientStock : null,
                applyTransition: inv => inv.ReleaseReservation(qty),
                ct: ct);
    }

    public Task<Result<InventoryStockResult>> ConfirmSaleAsync(
        Guid skuId, Guid shopId, int qty, string refType, Guid refId, Guid operationId, Guid? userId, CancellationToken ct = default)
    {
        return qty <= 0
            ? Task.FromResult<Result<InventoryStockResult>>(InventoryErrors.InvalidQtyChange)
            : ExecuteStockTransitionAsync(
                skuId, shopId, operationId, userId, refType, refId, $"Xuất bán đơn hàng {refId}",
                InventoryChangeType.SaleConfirmed, qtyChange: -qty, reservedChange: -qty,
                validatePrecondition: inv => (inv.ReservedQty < qty || inv.QtyOnHand < qty) ? InventoryErrors.InsufficientStock : null,
                applyTransition: inv => inv.ConfirmSale(qty),
                ct: ct);
    }

    public async Task<Result<InventoryStockResult>> ReturnStockAsync(
        Guid skuId, Guid shopId, int qty, string refType, Guid refId, Guid operationId, Guid? userId, CancellationToken ct = default)
    {
        if (qty <= 0)
        {
            return InventoryErrors.InvalidQtyChange;
        }

        if (refId == Guid.Empty)
        {
            return InventoryErrors.MissingOrderReference;
        }

        // Kiểm tra chặt chẽ: chỉ cho phép hoàn trả số lượng đã bán thực tế của refId (Order)
        var confirmedSold = await dbContext.InventoryHistories
            .Where(h => h.SkuId == skuId && h.RefId == refId && h.ChangeType == InventoryChangeType.SaleConfirmed)
            .SumAsync(h => (int?)Math.Abs(h.QtyChange), ct) ?? 0;

        var alreadyRestored = await dbContext.InventoryHistories
            .Where(h => h.SkuId == skuId && h.RefId == refId && (h.ChangeType == InventoryChangeType.SaleCancelled || h.ChangeType == InventoryChangeType.ReturnIn))
            .SumAsync(h => (int?)h.QtyChange, ct) ?? 0;

        var eligibleQty = confirmedSold - alreadyRestored;
        if (confirmedSold == 0 || qty > eligibleQty)
        {
            return InventoryErrors.CannotReturnMoreThanSold;
        }

        return await ExecuteStockTransitionAsync(
            skuId, shopId, operationId, userId, refType, refId, $"Khách trả hàng đơn {refId}",
            InventoryChangeType.ReturnIn, qtyChange: qty, reservedChange: 0,
            validatePrecondition: _ => null,
            applyTransition: inv => inv.Return(qty),
            ct: ct);
    }

    public async Task<Result<InventoryStockResult>> CancelSaleAsync(
        Guid skuId, Guid shopId, int qty, string refType, Guid refId, Guid operationId, Guid? userId, CancellationToken ct = default)
    {
        if (qty <= 0)
        {
            return InventoryErrors.InvalidQtyChange;
        }

        if (refId == Guid.Empty)
        {
            return InventoryErrors.MissingOrderReference;
        }

        // Kiểm tra chặt chẽ: chỉ cho phép hủy bán số lượng đã confirm bán thực tế của refId
        var confirmedSold = await dbContext.InventoryHistories
            .Where(h => h.SkuId == skuId && h.RefId == refId && h.ChangeType == InventoryChangeType.SaleConfirmed)
            .SumAsync(h => (int?)Math.Abs(h.QtyChange), ct) ?? 0;

        var alreadyRestored = await dbContext.InventoryHistories
            .Where(h => h.SkuId == skuId && h.RefId == refId && (h.ChangeType == InventoryChangeType.SaleCancelled || h.ChangeType == InventoryChangeType.ReturnIn))
            .SumAsync(h => (int?)h.QtyChange, ct) ?? 0;

        var eligibleQty = confirmedSold - alreadyRestored;
        if (confirmedSold == 0 || qty > eligibleQty)
        {
            return InventoryErrors.CannotCancelMoreThanSold;
        }

        return await ExecuteStockTransitionAsync(
            skuId, shopId, operationId, userId, refType, refId, $"Hủy xuất bán đơn hàng {refId}",
            InventoryChangeType.SaleCancelled, qtyChange: qty, reservedChange: 0,
            validatePrecondition: _ => null,
            applyTransition: inv => inv.CancelSale(qty),
            ct: ct);
    }

    public Task<Result<InventoryStockResult>> AdjustStockAsync(
        Guid skuId, Guid shopId, int qtyChange, InventoryChangeType changeType, string? note, Guid operationId, Guid? userId, CancellationToken ct = default)
    {
        return qtyChange == 0
            ? Task.FromResult<Result<InventoryStockResult>>(InventoryErrors.InvalidQtyChange)
            : ExecuteStockTransitionAsync(
                skuId, shopId, operationId, userId, "ManualAdjustment", null, note,
                changeType, qtyChange: qtyChange, reservedChange: 0,
                validatePrecondition: inv => (inv.QtyOnHand + qtyChange < inv.ReservedQty || inv.QtyOnHand + qtyChange < 0) ? InventoryErrors.InsufficientStock : null,
                applyTransition: inv => inv.AdjustOnHand(qtyChange),
                ct: ct);
    }

    private async Task<Result<InventoryStockResult>> ExecuteStockTransitionAsync(
        Guid skuId,
        Guid shopId,
        Guid operationId,
        Guid? userId,
        string refType,
        Guid? refId,
        string? note,
        InventoryChangeType changeType,
        int qtyChange,
        int reservedChange,
        Func<Inventory, Error?> validatePrecondition,
        Action<Inventory> applyTransition,
        CancellationToken ct)
    {
        // 1. Idempotency Check: nếu operationId đã được xử lý trước đó, trả về snapshot hiện tại
        var existingHistory = await dbContext.InventoryHistories
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.OperationId == operationId, ct);

        if (existingHistory is not null)
        {
            var existingInv = await dbContext.Inventories
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == existingHistory.InventoryId, ct);

            if (existingInv is not null)
            {
                return new InventoryStockResult(
                    existingInv.Id,
                    existingInv.SkuId,
                    existingInv.QtyOnHand,
                    existingInv.ReservedQty,
                    existingInv.AvailableQty,
                    existingInv.MinStock,
                    existingInv.Version,
                    AlreadyProcessed: true);
            }
        }

        // 2. Nạp entity cần cập nhật
        var inventory = await dbContext.Inventories
            .FirstOrDefaultAsync(i => i.SkuId == skuId && i.ShopId == shopId, ct);

        if (inventory is null)
        {
            return InventoryErrors.NotFound;
        }

        // 3. Kiểm tra điều kiện tiên quyết (Precondition check)
        var validationError = validatePrecondition(inventory);
        if (validationError is not null)
        {
            return validationError;
        }

        var qtyBefore = inventory.QtyOnHand;
        var reservedBefore = inventory.ReservedQty;

        // 4. Áp dụng State Machine
        applyTransition(inventory);

        // 5. Ghi Sổ cái bất biến (Immutable Ledger)
        var history = new InventoryHistory(
            inventoryId: inventory.Id,
            skuId: skuId,
            changeType: changeType,
            qtyBefore: qtyBefore,
            qtyChange: qtyChange,
            reservedBefore: reservedBefore,
            reservedChange: reservedChange,
            qtyAfter: inventory.QtyOnHand,
            operationId: operationId,
            refType: refType,
            refId: refId,
            note: note,
            createdBy: userId);

        await dbContext.InventoryHistories.AddAsync(history, ct);

        return new InventoryStockResult(
            inventory.Id,
            inventory.SkuId,
            inventory.QtyOnHand,
            inventory.ReservedQty,
            inventory.AvailableQty,
            inventory.MinStock,
            inventory.Version);
    }
}
