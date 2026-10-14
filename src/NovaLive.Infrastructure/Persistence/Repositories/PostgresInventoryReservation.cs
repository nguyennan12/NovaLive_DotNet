using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.UseCases.Inventory;
using NovaLive.Domain.Common;
using NovaLive.Domain.Inventory;
using Npgsql;

namespace NovaLive.Infrastructure.Persistence.Repositories;

internal static class PostgresInventoryReservation
{
    public static async Task<Result<InventoryStockResult>> ReserveAsync(DbContext db,
        Guid skuId, Guid shopId, int qty, string refType, Guid refId,
        Guid operationId, Guid? userId, CancellationToken ct)
    {
        // Raw SQL must never overwrite a pending tracked stock transition.
        var tracked = db.ChangeTracker.Entries<Inventory>()
            .SingleOrDefault(e => e.Entity.SkuId == skuId && e.Entity.ShopId == shopId);
        if (tracked is not null && tracked.State != EntityState.Unchanged)
            return InventoryErrors.PendingChanges;

        var replay = await ReadReplayAsync(db, operationId, ct);
        if (replay is not null)
        {
            if (tracked is not null) await tracked.ReloadAsync(ct);
            return replay;
        }

        var transaction = db.Database.CurrentTransaction;
        var savepoint = "reserve_" + Guid.NewGuid().ToString("N");
        if (transaction is not null) await transaction.CreateSavepointAsync(savepoint, ct);
        List<InventoryStockResult> rows;
        try
        {
            // One statement: a failed ledger insert also rolls back the stock update.
            // PostgreSQL rechecks the predicate after waiting for a concurrent row update.
            rows = await db.Database.SqlQuery<InventoryStockResult>($"""
                WITH changed AS (
                    UPDATE public.inventories
                    SET reserved_qty = reserved_qty + {qty}, version = version + 1,
                        last_updated = statement_timestamp()
                    WHERE sku_id = {skuId} AND shop_id = {shopId}
                      AND qty_on_hand - reserved_qty >= {qty}
                    RETURNING id, sku_id, qty_on_hand, reserved_qty, min_stock, version
                ), ledger AS (
                    INSERT INTO public.inventory_histories
                        (id, inventory_id, sku_id, operation_id, change_type,
                         qty_before, qty_change, reserved_before, reserved_change, qty_after,
                         ref_type, ref_id, note, created_by, created_at)
                    SELECT {Guid.NewGuid()}, id, sku_id, {operationId}, 'ReserveAdd',
                           qty_on_hand, 0, reserved_qty - {qty}, {qty}, qty_on_hand,
                           {refType}, {refId}, {$"Giữ chỗ đơn hàng {refId}"}, {userId}, statement_timestamp()
                    FROM changed
                    RETURNING inventory_id
                )
                SELECT c.id AS inventory_id, c.sku_id, c.qty_on_hand,
                       c.reserved_qty, c.qty_on_hand - c.reserved_qty AS available_qty,
                       c.min_stock, c.version, FALSE AS already_processed
                FROM changed c JOIN ledger l ON l.inventory_id = c.id
                """).ToListAsync(ct);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation
            && ex.ConstraintName == "ix_inventory_histories_operation_id")
        {
            // A duplicate request may race the initial replay read. Undo its entire
            // statement and restore the caller transaction before reading the winner.
            if (transaction is not null)
            {
                await transaction.RollbackToSavepointAsync(savepoint, ct);
                await transaction.ReleaseSavepointAsync(savepoint, ct);
            }
            var winner = await ReadReplayAsync(db, operationId, ct);
            if (winner is not null)
            {
                if (tracked is not null) await tracked.ReloadAsync(ct);
                return winner;
            }
            throw;
        }
        if (transaction is not null) await transaction.ReleaseSavepointAsync(savepoint, ct);
        if (tracked is not null) await tracked.ReloadAsync(ct);
        if (rows.Count == 1) return rows[0];

        // A duplicate request can also find the stock exhausted by its first attempt.
        replay = await ReadReplayAsync(db, operationId, ct);
        if (replay is not null) return replay;
        return await db.Set<Inventory>().AnyAsync(i => i.SkuId == skuId && i.ShopId == shopId, ct)
            ? InventoryErrors.InsufficientStock : InventoryErrors.NotFound;
    }

    private static async Task<InventoryStockResult?> ReadReplayAsync(DbContext db, Guid operationId, CancellationToken ct)
    {
        var history = await db.Set<InventoryHistory>().AsNoTracking()
            .SingleOrDefaultAsync(h => h.OperationId == operationId, ct);
        if (history is null) return null;
        var stock = await db.Set<Inventory>().AsNoTracking().SingleAsync(i => i.Id == history.InventoryId, ct);
        return new(stock.Id, stock.SkuId, stock.QtyOnHand, stock.ReservedQty,
            stock.AvailableQty, stock.MinStock, stock.Version, AlreadyProcessed: true);
    }
}
