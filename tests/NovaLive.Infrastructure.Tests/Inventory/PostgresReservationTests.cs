using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.UseCases.Inventory;
using NovaLive.Domain.Common;
using NovaLive.Domain.Products;
using NovaLive.Infrastructure.Persistence;
using NovaLive.Infrastructure.Tests.Auth;

namespace NovaLive.Infrastructure.Tests.Inventory;

public sealed class PostgresReservationTests
{
    private static async Task<Sku> SeedAsync(AuthTestHost host, int quantity)
    {
        var sku = new Sku(Guid.NewGuid(), Guid.NewGuid(), "RESERVE", null, 10m, 10m);
        await host.WithDb(async db =>
        {
            db.Skus.Add(sku);
            db.Inventories.Add(new Domain.Inventory.Inventory(sku.Id, sku.ShopId, quantity));
            await db.SaveChangesAsync();
        });
        return sku;
    }

    [PostgresFact]
    public async Task ConcurrentReservations_RecordExactLedgerChain_AndFailuresWriteNothing()
    {
        await using var host = new AuthTestHost(); await host.InitializeAsync();
        var sku = await SeedAsync(host, 5);
        var results = await Task.WhenAll(Enumerable.Range(0, 20).Select(async _ =>
        {
            using var scope = host.Services.CreateScope();
            return await scope.ServiceProvider.GetRequiredService<IInventoryRepository>()
                .ReserveStockAsync(sku.Id, sku.ShopId, 1, "Order", Guid.NewGuid(), Guid.NewGuid(), null);
        }));
        results.Count(r => r.IsSuccess).Should().Be(5);
        results.Where(r => r.IsFailure).Should().HaveCount(15)
            .And.OnlyContain(r => r.Error == InventoryErrors.InsufficientStock);
        await host.WithDb(async db =>
        {
            var histories = await db.InventoryHistories.OrderBy(h => h.ReservedBefore).ToListAsync();
            histories.Should().HaveCount(5);
            histories.Select(h => h.ReservedBefore).Should().Equal(0, 1, 2, 3, 4);
            histories.Should().OnlyContain(h => h.ChangeType == InventoryChangeType.ReserveAdd
                && h.QtyBefore == 5 && h.QtyAfter == 5 && h.QtyChange == 0 && h.ReservedChange == 1);
            var stock = await db.Inventories.SingleAsync();
            stock.ReservedQty.Should().Be(5); stock.QtyOnHand.Should().Be(5); stock.Version.Should().Be(6);
        });
    }

    [PostgresFact]
    public async Task CallerRollback_UndoesStockAndLedger_AndTrackedStockCanBeReleased()
    {
        await using var host = new AuthTestHost(); await host.InitializeAsync();
        var sku = await SeedAsync(host, 5);
        using (var scope = host.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var repo = scope.ServiceProvider.GetRequiredService<IInventoryRepository>();
            var tracked = await db.Inventories.SingleAsync();
            await using var tx = await db.Database.BeginTransactionAsync();
            (await repo.ReserveStockAsync(sku.Id, sku.ShopId, 3, "Order", Guid.NewGuid(), Guid.NewGuid(), null)).IsSuccess.Should().BeTrue();
            tracked.ReservedQty.Should().Be(3); tracked.Version.Should().Be(2);
            (await repo.ReleaseStockAsync(sku.Id, sku.ShopId, 1, "Order", Guid.NewGuid(), Guid.NewGuid(), null)).IsSuccess.Should().BeTrue();
            await db.SaveChangesAsync();
            (await db.InventoryHistories.CountAsync()).Should().Be(2);
            await tx.RollbackAsync();
        }
        await host.WithDb(async db =>
        {
            (await db.Inventories.SingleAsync()).ReservedQty.Should().Be(0);
            (await db.InventoryHistories.CountAsync()).Should().Be(0);
        });
    }

    [PostgresFact]
    public async Task ConcurrentDuplicateOperations_ReserveOnce_AndCallerTransactionsRemainUsable()
    {
        await using var host = new AuthTestHost(); await host.InitializeAsync();
        var sku = await SeedAsync(host, 20); var operation = Guid.NewGuid(); var order = Guid.NewGuid();
        var results = await Task.WhenAll(Enumerable.Range(0, 12).Select(async _ =>
        {
            using var scope = host.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await using var tx = await db.Database.BeginTransactionAsync();
            var result = await scope.ServiceProvider.GetRequiredService<IInventoryRepository>()
                .ReserveStockAsync(sku.Id, sku.ShopId, 1, "Order", order, operation, null);
            (await db.InventoryHistories.CountAsync()).Should().Be(1);
            await db.SaveChangesAsync(); await tx.CommitAsync();
            return result;
        }));
        results.Should().OnlyContain(r => r.IsSuccess);
        results.Count(r => !r.Value!.AlreadyProcessed).Should().Be(1);
        await host.WithDb(async db =>
        {
            (await db.Inventories.SingleAsync()).ReservedQty.Should().Be(1);
            (await db.InventoryHistories.CountAsync()).Should().Be(1);
        });
    }

    [PostgresFact]
    public async Task PendingTrackedChange_IsPreserved_AndCanBeSavedBeforeReservation()
    {
        await using var host = new AuthTestHost(); await host.InitializeAsync();
        var sku = await SeedAsync(host, 5);
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var repo = scope.ServiceProvider.GetRequiredService<IInventoryRepository>();
        await using var tx = await db.Database.BeginTransactionAsync();
        var stock = await db.Inventories.SingleAsync(); stock.AdjustOnHand(1);
        (await repo.ReserveStockAsync(sku.Id, sku.ShopId, 1, "Order", Guid.NewGuid(), Guid.NewGuid(), null))
            .Error.Should().Be(InventoryErrors.PendingChanges);
        stock.QtyOnHand.Should().Be(6);
        (await db.InventoryHistories.CountAsync()).Should().Be(0);
        await db.SaveChangesAsync();
        (await repo.ReserveStockAsync(sku.Id, sku.ShopId, 1, "Order", Guid.NewGuid(), Guid.NewGuid(), null)).IsSuccess.Should().BeTrue();
        await tx.RollbackAsync();
    }
}
