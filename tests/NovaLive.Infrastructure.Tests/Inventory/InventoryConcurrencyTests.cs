using System.Collections.Concurrent;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Domain.Common;
using NovaLive.Domain.Inventory;
using NovaLive.Domain.Products;
using NovaLive.Domain.Rbac;
using NovaLive.Domain.Shops;
using NovaLive.Domain.Users;
using NovaLive.Infrastructure.Persistence.Seeding;
using NovaLive.Infrastructure.Tests.Auth;

namespace NovaLive.Infrastructure.Tests.Inventory;

public sealed class InventoryConcurrencyTests
{
    private static async Task<(Guid SellerId, Guid ShopId, Guid SkuId)> SeedStockFixtureAsync(
        AuthTestHost host,
        int initialStock = 10)
    {
        var seller = new User("seller_concurrency@example.com", "hash:Password123", "Seller Concurrency", "0977777777");
        seller.Activate();

        var shop = new Shop(seller.Id, "Concurrency Store", "concurrency-store");
        shop.Approve();

        var category = new Category(Guid.NewGuid(), "Thời trang", "thoi-trang");

        var spu = new Spu(
            shopId: shop.Id,
            categoryId: category.Id,
            name: "Flash Sale T-Shirt",
            description: "Limited stock item",
            brand: "Nova",
            thumbnailUrl: null,
            attributesConfigJson: "[]");

        var sku = new Sku(
            spuId: spu.Id,
            shopId: shop.Id,
            skuCode: "FLASH-TSHIRT-01",
            attributesJson: "{}",
            originalPrice: 100000m,
            sellPrice: 50000m,
            weightGram: 150,
            isActive: true);

        var inventory = new Domain.Inventory.Inventory(
            skuId: sku.Id,
            shopId: shop.Id,
            initialStock: initialStock,
            minStock: 2);

        await host.WithDb(async db =>
        {
            var seeder = new RbacDataSeeder(db, host.Clock, NullLogger<RbacDataSeeder>.Instance);
            await seeder.SeedAsync(CancellationToken.None);

            db.Users.Add(seller);
            db.UserRoles.Add(new UserRole(seller.Id, SystemRoleIds.Seller, host.Clock.UtcNow));
            db.Shops.Add(shop);
            db.Categories.Add(category);
            db.Spus.Add(spu);
            db.Skus.Add(sku);
            db.Inventories.Add(inventory);
            await db.SaveChangesAsync();
        });

        return (seller.Id, shop.Id, sku.Id);
    }

    [Fact]
    public async Task StockStateMachine_FullOrderLifecycle_MaintainsIntegrityAndLedger()
    {
        // Arrange
        await using var host = new AuthTestHost();
        await host.InitializeAsync();
        var (sellerId, shopId, skuId) = await SeedStockFixtureAsync(host, initialStock: 10);

        using var scope = host.Services.CreateScope();
        var inventoryRepo = scope.ServiceProvider.GetRequiredService<IInventoryRepository>();
        var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

        var orderId = Guid.NewGuid();
        var opReserve = Guid.NewGuid();
        var opConfirm = Guid.NewGuid();

        // 1. Reserve 3 items at checkout
        var reserveResult = await inventoryRepo.ReserveStockAsync(
            skuId: skuId,
            shopId: shopId,
            qty: 3,
            refType: "Order",
            refId: orderId,
            operationId: opReserve,
            userId: sellerId);

        reserveResult.IsSuccess.Should().BeTrue();
        reserveResult.Value!.QtyOnHand.Should().Be(10);
        reserveResult.Value.ReservedQty.Should().Be(3);
        reserveResult.Value.AvailableQty.Should().Be(7);
        await ((DbContext)db).SaveChangesAsync();

        // 2. Idempotency test: Re-send same reserve operation
        var retryReserve = await inventoryRepo.ReserveStockAsync(
            skuId: skuId,
            shopId: shopId,
            qty: 3,
            refType: "Order",
            refId: orderId,
            operationId: opReserve,
            userId: sellerId);

        retryReserve.IsSuccess.Should().BeTrue();
        retryReserve.Value!.AlreadyProcessed.Should().BeTrue();
        retryReserve.Value.ReservedQty.Should().Be(3);

        // 3. Confirm sale (payment success) -> QtyOnHand drops to 7, ReservedQty drops to 0
        var confirmResult = await inventoryRepo.ConfirmSaleAsync(
            skuId: skuId,
            shopId: shopId,
            qty: 3,
            refType: "Order",
            refId: orderId,
            operationId: opConfirm,
            userId: sellerId);

        confirmResult.IsSuccess.Should().BeTrue();
        confirmResult.Value!.QtyOnHand.Should().Be(7);
        confirmResult.Value.ReservedQty.Should().Be(0);
        confirmResult.Value.AvailableQty.Should().Be(7);
        await ((DbContext)db).SaveChangesAsync();

        // 4. Verify Ledger count (1 reserve + 1 sale confirmed = 2 total)
        var histories = await db.InventoryHistories
            .Where(h => h.SkuId == skuId)
            .OrderBy(h => h.CreatedAt)
            .ToListAsync();

        histories.Should().HaveCount(2); // 1 ReserveAdd + 1 SaleConfirmed
        histories[0].ChangeType.Should().Be(InventoryChangeType.ReserveAdd);
        histories[0].ReservedChange.Should().Be(3);
        histories[1].ChangeType.Should().Be(InventoryChangeType.SaleConfirmed);
        histories[1].QtyChange.Should().Be(-3);
        histories[1].ReservedChange.Should().Be(-3);
    }

    [Fact]
    public async Task StockStateMachine_ReleaseReservation_RestoresAvailableStock()
    {
        // Arrange
        await using var host = new AuthTestHost();
        await host.InitializeAsync();
        var (sellerId, shopId, skuId) = await SeedStockFixtureAsync(host, initialStock: 5);

        using var scope = host.Services.CreateScope();
        var inventoryRepo = scope.ServiceProvider.GetRequiredService<IInventoryRepository>();
        var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

        var orderId = Guid.NewGuid();

        // Reserve 4 items
        await inventoryRepo.ReserveStockAsync(skuId, shopId, 4, "Order", orderId, Guid.NewGuid(), sellerId);
        await ((DbContext)db).SaveChangesAsync();

        // Release 4 items (order cancelled / timeout)
        var releaseResult = await inventoryRepo.ReleaseStockAsync(skuId, shopId, 4, "Order", orderId, Guid.NewGuid(), sellerId);
        await ((DbContext)db).SaveChangesAsync();

        // Assert
        releaseResult.IsSuccess.Should().BeTrue();
        releaseResult.Value!.QtyOnHand.Should().Be(5);
        releaseResult.Value.ReservedQty.Should().Be(0);
        releaseResult.Value.AvailableQty.Should().Be(5);
    }

    [Fact]
    public async Task StockStateMachine_CustomerReturn_IncreasesOnHandStock()
    {
        // Arrange
        await using var host = new AuthTestHost();
        await host.InitializeAsync();
        var (sellerId, shopId, skuId) = await SeedStockFixtureAsync(host, initialStock: 5);

        using var scope = host.Services.CreateScope();
        var inventoryRepo = scope.ServiceProvider.GetRequiredService<IInventoryRepository>();
        var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

        var returnId = Guid.NewGuid();

        // Return 2 items
        var returnResult = await inventoryRepo.ReturnStockAsync(skuId, shopId, 2, "Return", returnId, Guid.NewGuid(), sellerId);
        await ((DbContext)db).SaveChangesAsync();

        // Assert
        returnResult.IsSuccess.Should().BeTrue();
        returnResult.Value!.QtyOnHand.Should().Be(7);
        returnResult.Value.ReservedQty.Should().Be(0);
        returnResult.Value.AvailableQty.Should().Be(7);
    }

    [Fact]
    public async Task ConcurrentStockReservation_NeverOversells_AndMaintainsZeroNegativeStock()
    {
        // Arrange: SKU has exactly 5 items in stock
        await using var host = new AuthTestHost();
        await host.InitializeAsync();
        var (sellerId, shopId, skuId) = await SeedStockFixtureAsync(host, initialStock: 5);

        var successCount = 0;
        var failureCount = 0;
        var totalAttempts = 20;

        // Act: 20 concurrent tasks attempt to reserve 1 item each
        var tasks = Enumerable.Range(0, totalAttempts).Select(async i =>
        {
            using var scope = host.Services.CreateScope();
            var inventoryRepo = scope.ServiceProvider.GetRequiredService<IInventoryRepository>();
            var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

            var orderId = Guid.NewGuid();
            var opId = Guid.NewGuid();

            var result = await inventoryRepo.ReserveStockAsync(
                skuId: skuId,
                shopId: shopId,
                qty: 1,
                refType: "Order",
                refId: orderId,
                operationId: opId,
                userId: sellerId);

            if (result.IsSuccess)
            {
                await ((DbContext)db).SaveChangesAsync();
                Interlocked.Increment(ref successCount);
            }
            else
            {
                Interlocked.Increment(ref failureCount);
            }
        });

        await Task.WhenAll(tasks);

        // Assert
        // Exactly 5 reservations must succeed, 15 must fail due to InsufficientStock
        successCount.Should().Be(5);
        failureCount.Should().Be(15);

        await host.WithDb(async db =>
        {
            var inv = await db.Inventories.SingleAsync(i => i.SkuId == skuId);
            inv.QtyOnHand.Should().Be(5);
            inv.ReservedQty.Should().Be(5);
            inv.AvailableQty.Should().Be(0);

            var ledgerEntries = await db.InventoryHistories.CountAsync(h => h.SkuId == skuId);
            ledgerEntries.Should().Be(5); // Exactly 5 ledger entries
        });
    }
}
