using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.UseCases.FlashSales;
using NovaLive.Application.UseCases.Orders.Commands.Checkout;
using NovaLive.Application.UseCases.Orders.Commands.CancelOrder;
using NovaLive.Contracts.V1.Orders;
using NovaLive.Domain.Common;
using NovaLive.Domain.FlashSales;
using NovaLive.Domain.Orders;
using NovaLive.Domain.Pricing;
using NovaLive.Infrastructure.Persistence;
using NovaLive.Infrastructure.Persistence.Repositories;
using NovaLive.Infrastructure.Tests.Auth;
using NovaLive.Infrastructure.Tests.Orders;
using Npgsql;

namespace NovaLive.Infrastructure.Tests.FlashSales;

public sealed class PostgresFlashSaleTests
{
    private static async Task<FlashSaleItem> SeedAsync(AuthTestHost host, Guid? skuId = null, Guid? shopId = null, int limit = 10)
    {
        FlashSaleItem item = null!;
        await host.WithDb(async db =>
        {
            var campaign = new FlashSaleCampaign("Concurrency", DateTimeOffset.UtcNow.AddHours(-1), DateTimeOffset.UtcNow.AddHours(1));
            db.FlashSaleCampaigns.Add(campaign);
            db.Entry(campaign).Property(c => c.Status).CurrentValue = FlashSaleCampaignStatus.Active;
            item = new FlashSaleItem(campaign.Id, skuId ?? Guid.NewGuid(), shopId ?? Guid.NewGuid(), 80, 10, limit);
            item.Approve(); db.FlashSaleItems.Add(item);
            await db.SaveChangesAsync();
        });
        return item;
    }

    [PostgresFact]
    public Task ThousandRequestsForTenSlots_ExactlyTenSucceed() => ContentionAsync(1000, 1, 10);

    [PostgresFact]
    public Task RequestsOfThree_NeverExceedTenSlots() => ContentionAsync(100, 3, 3);

    private static async Task ContentionAsync(int requests, int quantity, int successes)
    {
        await using var host = new AuthTestHost(); await host.InitializeAsync(CheckoutPersistenceTests.Register);
        var item = await SeedAsync(host);
        string connection = null!;
        await host.WithDb(db => { connection = db.Database.GetConnectionString()!; return Task.CompletedTask; });
        // 1000 independently scoped contexts contend through a pool, as in the API under load.
        // A bounded connection pool prevents server connection exhaustion from masking stock correctness.
        await using var source = NpgsqlDataSource.Create(new NpgsqlConnectionStringBuilder(connection)
            { Pooling = true, MaxPoolSize = 32, Timeout = 60 }.ConnectionString);
        var options = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(source).UseSnakeCaseNamingConvention().Options;
        var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var tasks = Enumerable.Range(0, requests).Select(async _ =>
        {
            await start.Task;
            await using var db = new AppDbContext(options);
            await using var transaction = await db.Database.BeginTransactionAsync();
            var reserved = await new FlashSaleRepository(db).TryReserveAsync(item.Id, quantity);
            await transaction.CommitAsync();
            return reserved;
        }).ToArray();
        start.SetResult();
        var results = await Task.WhenAll(tasks);
        results.Count(r => r).Should().Be(successes);
        results.Count(r => !r).Should().Be(requests - successes);
        await host.WithDb(async db =>
        {
            var stock = await db.FlashSaleItems.SingleAsync(i => i.Id == item.Id);
            stock.ReservedQty.Should().Be(successes * quantity); stock.SoldQty.Should().Be(0);
            (stock.ReservedQty + stock.SoldQty).Should().BeLessThanOrEqualTo(stock.Quantity);
        });
    }

    [PostgresFact]
    public async Task SameBuyerParallelRequests_AdvisoryLockProtectsLimit()
    {
        await using var host = new AuthTestHost(); await host.InitializeAsync(CheckoutPersistenceTests.Register);
        var item = await SeedAsync(host, limit: 1); var buyer = Guid.NewGuid();
        var results = await Task.WhenAll(Enumerable.Range(0, 2).Select(async _ =>
        {
            using var scope = host.Services.CreateScope(); var sp = scope.ServiceProvider;
            var tx = sp.GetRequiredService<ITransactionManager>();
            var flash = sp.GetRequiredService<IFlashSaleRepository>(); var orders = sp.GetRequiredService<IOrderItemRepository>();
            return await tx.ExecuteAsync<Result>(async () =>
            {
                await flash.LockBuyerItemAsync(buyer, item.Id);
                if (await orders.GetFlashSaleUsageAsync(buyer, item.Id, default) >= item.PerUserLimit)
                    return FlashSalesErrors.PerUserLimitExceeded;
                if (!await flash.TryReserveAsync(item.Id, 1)) return FlashSalesErrors.SoldOut;
                var now = DateTimeOffset.UtcNow;
                var price = PriceCalculator.Calculate(new PriceCalculationInput(
                    [new ShopCalculationInput(item.ShopId, 0, [new OrderItemCalculationInput(item.SkuId, Guid.NewGuid(), Guid.NewGuid(), 1, 80)], null)],
                    null, null, now)).Value!.ShopResults.Single();
                var parent = ParentOrder.Create(buyer, "TEST-" + Guid.NewGuid().ToString("N"), "{}", 80, 0, 0, 80, null, now);
                var sub = SubOrder.Create(parent.Id, parent.OrderCode + "-S1", Guid.NewGuid(), price, false, now, OrderSource.FlashSale);
                var db = sp.GetRequiredService<AppDbContext>(); db.ParentOrders.Add(parent); db.SubOrders.Add(sub);
                await orders.AddAsync(OrderItem.Create(sub.Id, "{}", 100, price.Items.Single(), item.Id), default);
                return Result.Success();
            }, false, default);
        }));
        results.Count(r => r.IsSuccess).Should().Be(1);
        results.Single(r => r.IsFailure).Error.Should().Be(FlashSalesErrors.PerUserLimitExceeded);
        await host.WithDb(async db =>
        {
            (await db.FlashSaleItems.SingleAsync()).ReservedQty.Should().Be(1);
            (await db.OrderItems.CountAsync()).Should().Be(1);
        });
    }

    [PostgresFact]
    public async Task ConfirmAndRelease_RetriesAndCompetingOperations_DoNotConsumeOtherOrdersQuota()
    {
        await using var host = new AuthTestHost(); host.Clock.UtcNow = DateTimeOffset.UtcNow;
        await host.InitializeAsync(CheckoutPersistenceTests.Register);
        var buyer = await host.AddUser(); host.Authenticate(buyer); var request = await CheckoutPersistenceTests.Seed(host, buyer);
        Guid sku = default, shop = default;
        await host.WithDb(async db => { var s = await db.Skus.SingleAsync(); sku = s.Id; shop = s.ShopId; });
        var item = await SeedAsync(host, sku, shop);
        var checkout = await host.Send(new CheckoutCommand(request, "flash-confirm")); checkout.IsSuccess.Should().BeTrue();
        await host.WithDb(async db =>
        {
            await using var tx = await db.Database.BeginTransactionAsync();
            (await new FlashSaleRepository(db).TryReserveAsync(item.Id, 3)).Should().BeTrue();
            await tx.CommitAsync();
        });
        var subIds = checkout.Value!.SubOrders.Select(s => s.Id).ToArray();
        async Task<Result> Transition(bool confirm)
        {
            using var scope = host.Services.CreateScope(); var sp = scope.ServiceProvider;
            return await sp.GetRequiredService<ITransactionManager>().ExecuteAsync(
                () => confirm ? sp.GetRequiredService<IFlashSaleStockService>().ConfirmSoldAsync(subIds)
                    : sp.GetRequiredService<IFlashSaleStockService>().ReleaseReservedAsync(subIds), false, default);
        }
        var results = await Task.WhenAll(Enumerable.Range(0, 10).Select(_ => Transition(true)));
        results.Should().OnlyContain(r => r.IsSuccess);
        (await Transition(false)).Error.Should().Be(FlashSalesErrors.ReservationConflict);
        await host.WithDb(async db =>
        {
            var stock = await db.FlashSaleItems.SingleAsync(); stock.ReservedQty.Should().Be(3); stock.SoldQty.Should().Be(1);
            (await db.OrderItems.SingleAsync()).FlashSaleState.Should().Be(FlashSaleReservationState.Sold);
            await using var tx = await db.Database.BeginTransactionAsync();
            var repo = new FlashSaleRepository(db);
            (await repo.ReleaseReservedAsync(item.Id, 4)).Should().BeFalse();
            (await repo.ConfirmSoldAsync(item.Id, 4)).Should().BeFalse();
            await tx.RollbackAsync();
        });
    }

    [PostgresFact]
    public async Task BuyerCancellation_ReleasesQuotaAndUserUsage_ExactlyOnce()
    {
        await using var host = new AuthTestHost(); host.Clock.UtcNow = DateTimeOffset.UtcNow;
        await host.InitializeAsync(CheckoutPersistenceTests.Register);
        var buyer = await host.AddUser(); host.Authenticate(buyer); var request = await CheckoutPersistenceTests.Seed(host, buyer);
        FlashSaleItem item = null!;
        await host.WithDb(async db => { var sku = await db.Skus.SingleAsync(); item = await SeedInContextAsync(db, sku.Id, sku.ShopId); });
        var checkout = await host.Send(new CheckoutCommand(request, "flash-cancel")); checkout.IsSuccess.Should().BeTrue();
        var cancel = await host.Send(new CancelOrderCommand(checkout.Value!.ParentOrderId, new CancelOrderRequest("Changed mind")));
        cancel.IsSuccess.Should().BeTrue();
        using var scope = host.Services.CreateScope(); var sp = scope.ServiceProvider;
        (await sp.GetRequiredService<ITransactionManager>().ExecuteAsync(
            () => sp.GetRequiredService<IFlashSaleStockService>().ReleaseReservedAsync(checkout.Value.SubOrders.Select(s => s.Id)), false, default))
            .IsSuccess.Should().BeTrue();
        (await sp.GetRequiredService<IOrderItemRepository>().GetFlashSaleUsageAsync(buyer, item.Id, default)).Should().Be(0);
        await host.WithDb(async db =>
        {
            var flash = await db.FlashSaleItems.SingleAsync(); flash.ReservedQty.Should().Be(0); flash.SoldQty.Should().Be(0);
            (await db.Inventories.SingleAsync()).ReservedQty.Should().Be(0);
            (await db.OrderItems.SingleAsync()).FlashSaleState.Should().Be(FlashSaleReservationState.Released);
        });
    }

    private static async Task<FlashSaleItem> SeedInContextAsync(AppDbContext db, Guid sku, Guid shop)
    {
        var campaign = new FlashSaleCampaign("Checkout", DateTimeOffset.UtcNow.AddHours(-1), DateTimeOffset.UtcNow.AddHours(1));
        db.FlashSaleCampaigns.Add(campaign); db.Entry(campaign).Property(c => c.Status).CurrentValue = FlashSaleCampaignStatus.Active;
        var item = new FlashSaleItem(campaign.Id, sku, shop, 80, 10, 10); item.Approve(); db.FlashSaleItems.Add(item);
        await db.SaveChangesAsync(); return item;
    }

    [PostgresFact]
    public async Task SecondFlashItemSoldOut_ResultFailureRollsBackEarlierFlashReserve()
    {
        await using var host = new AuthTestHost(); host.Clock.UtcNow = DateTimeOffset.UtcNow;
        await host.InitializeAsync(CheckoutPersistenceTests.Register);
        var buyer = await host.AddUser(); host.Authenticate(buyer); var request = await CheckoutPersistenceTests.Seed(host, buyer);
        Guid availableItem = default, exhaustedItem = default;
        await host.WithDb(async db =>
        {
            var firstSku = await db.Skus.SingleAsync();
            var first = await SeedInContextAsync(db, firstSku.Id, firstSku.ShopId);
            var secondSku = new Domain.Products.Sku(firstSku.SpuId, firstSku.ShopId, "SECOND", "{}", 100, 100);
            db.Skus.Add(secondSku); db.Inventories.Add(new Domain.Inventory.Inventory(secondSku.Id, firstSku.ShopId, 10));
            var second = new FlashSaleItem(first.CampaignId, secondSku.Id, firstSku.ShopId, 80, 10, 10); second.Approve();
            db.FlashSaleItems.Add(second);
            var cart = await db.Carts.SingleAsync();
            var cartItem = new Domain.Carts.CartItem(Guid.NewGuid(), cart.Id, secondSku.Id, secondSku.ShopId, 1, 1, host.Clock.UtcNow);
            db.CartItems.Add(cartItem); await db.SaveChangesAsync();
            request = request with { CartItemIds = request.CartItemIds.Append(cartItem.Id).ToArray() };
            var sorted = new[] { first, second }.OrderBy(i => i.Id).ToArray();
            availableItem = sorted[0].Id; exhaustedItem = sorted[1].Id;
            await using var tx = await db.Database.BeginTransactionAsync();
            (await new FlashSaleRepository(db).TryReserveAsync(exhaustedItem, 10)).Should().BeTrue(); await tx.CommitAsync();
        });
        var result = await host.Send(new CheckoutCommand(request, "two-flash-rollback"));
        result.Error.Should().Be(FlashSalesErrors.SoldOut);
        await host.WithDb(async db =>
        {
            (await db.FlashSaleItems.SingleAsync(i => i.Id == availableItem)).ReservedQty.Should().Be(0);
            (await db.FlashSaleItems.SingleAsync(i => i.Id == exhaustedItem)).ReservedQty.Should().Be(10);
            (await db.Inventories.ToListAsync()).Should().OnlyContain(i => i.ReservedQty == 0);
            (await db.ParentOrders.CountAsync()).Should().Be(0); (await db.Payments.CountAsync()).Should().Be(0);
            (await db.InventoryHistories.CountAsync()).Should().Be(0); (await db.CartItems.CountAsync()).Should().Be(2);
        });
    }

    [PostgresFact]
    public async Task ConcurrentReleaseRetries_PreserveOtherReservations_AndRollbackOnCounterMismatch()
    {
        await using var host = new AuthTestHost(); host.Clock.UtcNow = DateTimeOffset.UtcNow;
        await host.InitializeAsync(CheckoutPersistenceTests.Register);
        var buyer = await host.AddUser(); host.Authenticate(buyer); var request = await CheckoutPersistenceTests.Seed(host, buyer);
        await host.WithDb(async db => { var sku = await db.Skus.SingleAsync(); await SeedInContextAsync(db, sku.Id, sku.ShopId); });
        var checkout = await host.Send(new CheckoutCommand(request, "release-retry")); checkout.IsSuccess.Should().BeTrue();
        var subIds = checkout.Value!.SubOrders.Select(s => s.Id).ToArray();
        // A lost counter update must not leave an idempotency marker committed.
        using (var scope = host.Services.CreateScope())
        {
            var sp = scope.ServiceProvider; var db = sp.GetRequiredService<AppDbContext>();
            await db.FlashSaleItems.ExecuteUpdateAsync(s => s.SetProperty(i => i.ReservedQty, 0));
            var result = await sp.GetRequiredService<ITransactionManager>().ExecuteAsync(
                () => sp.GetRequiredService<IFlashSaleStockService>().ReleaseReservedAsync(subIds), false, default);
            result.Error.Should().Be(FlashSalesErrors.ReservationConflict);
            (await db.OrderItems.AsNoTracking().SingleAsync()).FlashSaleState.Should().Be(FlashSaleReservationState.Reserved);
            // Restore the order's reservation plus three slots held for other orders.
            await db.FlashSaleItems.ExecuteUpdateAsync(s => s.SetProperty(i => i.ReservedQty, 4));
        }
        var results = await Task.WhenAll(Enumerable.Range(0, 10).Select(async _ =>
        {
            using var scope = host.Services.CreateScope(); var sp = scope.ServiceProvider;
            return await sp.GetRequiredService<ITransactionManager>().ExecuteAsync(
                () => sp.GetRequiredService<IFlashSaleStockService>().ReleaseReservedAsync(subIds), false, default);
        }));
        results.Should().OnlyContain(r => r.IsSuccess);
        await host.WithDb(async db =>
        {
            var flash = await db.FlashSaleItems.SingleAsync(); flash.ReservedQty.Should().Be(3); flash.SoldQty.Should().Be(0);
            (await db.OrderItems.SingleAsync()).FlashSaleState.Should().Be(FlashSaleReservationState.Released);
        });
    }

    [PostgresFact]
    public async Task FailureAfterFlashAndInventoryReservation_RollsBackAllWrites()
    {
        await using var host = new AuthTestHost(); host.Clock.UtcNow = DateTimeOffset.UtcNow;
        await host.InitializeAsync(s =>
        {
            CheckoutPersistenceTests.Register(s);
            var outbox = new Mock<IOutboxRepository>();
            outbox.Setup(o => o.AddAsync(It.IsAny<Domain.System.OutboxMessage>(), It.IsAny<CancellationToken>())).ThrowsAsync(new IOException("Fail after reserves"));
            s.AddScoped(_ => outbox.Object);
        });
        var buyer = await host.AddUser(); host.Authenticate(buyer); var request = await CheckoutPersistenceTests.Seed(host, buyer);
        await host.WithDb(async db => { var sku = await db.Skus.SingleAsync(); await SeedInContextAsync(db, sku.Id, sku.ShopId); });
        await FluentActions.Awaiting(() => host.Send(new CheckoutCommand(request, "flash-rollback"))).Should().ThrowAsync<IOException>();
        await host.WithDb(async db =>
        {
            (await db.FlashSaleItems.SingleAsync()).ReservedQty.Should().Be(0);
            (await db.Inventories.SingleAsync()).ReservedQty.Should().Be(0);
            (await db.ParentOrders.CountAsync()).Should().Be(0); (await db.SubOrders.CountAsync()).Should().Be(0);
            (await db.OrderItems.CountAsync()).Should().Be(0); (await db.Payments.CountAsync()).Should().Be(0);
            (await db.InventoryHistories.CountAsync()).Should().Be(0); (await db.DiscountUsages.CountAsync()).Should().Be(0);
            (await db.CartItems.CountAsync()).Should().Be(1); (await db.Discounts.SingleAsync()).UsedCount.Should().Be(0);
        });
    }

    [PostgresFact]
    public async Task Reserve_RejectsInactiveExpiredUnapprovedAndInvalidQuantities()
    {
        await using var host = new AuthTestHost(); await host.InitializeAsync(CheckoutPersistenceTests.Register);
        var item = await SeedAsync(host);
        await host.WithDb(async db =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(); var repo = new FlashSaleRepository(db);
            (await repo.TryReserveAsync(item.Id, 0)).Should().BeFalse(); (await repo.TryReserveAsync(item.Id, -1)).Should().BeFalse();
            (await repo.TryReserveAsync(item.Id, 11)).Should().BeFalse();
            await db.FlashSaleCampaigns.ExecuteUpdateAsync(s => s.SetProperty(c => c.Status, FlashSaleCampaignStatus.Cancelled));
            (await repo.TryReserveAsync(item.Id, 1)).Should().BeFalse();
            await db.FlashSaleCampaigns.ExecuteUpdateAsync(s => s.SetProperty(c => c.Status, FlashSaleCampaignStatus.Active)
                .SetProperty(c => c.EndAt, DateTimeOffset.UtcNow.AddSeconds(-1)));
            (await repo.TryReserveAsync(item.Id, 1)).Should().BeFalse();
            await db.FlashSaleCampaigns.ExecuteUpdateAsync(s => s.SetProperty(c => c.EndAt, DateTimeOffset.UtcNow.AddHours(1)));
            await db.FlashSaleCampaigns.ExecuteUpdateAsync(s => s.SetProperty(c => c.Status, FlashSaleCampaignStatus.Ended));
            (await repo.TryReserveAsync(item.Id, 1)).Should().BeFalse();
            (await repo.GetActiveItemsBySkuIdsAsync([item.SkuId], DateTimeOffset.UtcNow)).Should().BeEmpty();
            await db.FlashSaleCampaigns.ExecuteUpdateAsync(s => s.SetProperty(c => c.Status, FlashSaleCampaignStatus.Scheduled)
                .SetProperty(c => c.StartAt, DateTimeOffset.UtcNow.AddMinutes(1)));
            (await repo.TryReserveAsync(item.Id, 1)).Should().BeFalse();
            await db.FlashSaleCampaigns.ExecuteUpdateAsync(s => s.SetProperty(c => c.StartAt, DateTimeOffset.UtcNow.AddMinutes(-1)));
            await db.FlashSaleItems.ExecuteUpdateAsync(s => s.SetProperty(i => i.Status, FlashSaleItemStatus.Pending));
            (await repo.TryReserveAsync(item.Id, 1)).Should().BeFalse();
            await tx.RollbackAsync();
        });
    }

    [PostgresFact]
    public async Task ScheduledCampaign_InCurrentWindow_UsesT26EffectiveActiveSemantics()
    {
        await using var host = new AuthTestHost(); host.Clock.UtcNow = DateTimeOffset.UtcNow;
        await host.InitializeAsync(CheckoutPersistenceTests.Register);
        var buyer = await host.AddUser(); host.Authenticate(buyer); var request = await CheckoutPersistenceTests.Seed(host, buyer);
        await host.WithDb(async db =>
        {
            var sku = await db.Skus.SingleAsync(); await SeedInContextAsync(db, sku.Id, sku.ShopId);
            await db.FlashSaleCampaigns.ExecuteUpdateAsync(s => s.SetProperty(c => c.Status, FlashSaleCampaignStatus.Scheduled));
        });
        (await host.Send(new CheckoutCommand(request, "scheduled-current"))).IsSuccess.Should().BeTrue();
        await host.WithDb(async db =>
        {
            (await db.OrderItems.SingleAsync()).UnitPrice.Should().Be(80);
            (await db.FlashSaleItems.SingleAsync()).ReservedQty.Should().Be(1);
        });
    }
}
