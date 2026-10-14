using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.UseCases.Carts.Commands.AddToCart;
using NovaLive.Contracts.V1.Carts;
using NovaLive.Domain.Carts;
using NovaLive.Domain.Common;
using NovaLive.Domain.Products;
using NovaLive.Domain.Shops;
using NovaLive.Infrastructure.Persistence;
using NovaLive.Infrastructure.Persistence.Repositories;
using NovaLive.Infrastructure.Tests.Auth;
namespace NovaLive.Infrastructure.Tests.Carts;
public sealed class PostgresCartConcurrencyTests
{
    [PostgresFact]
    public async Task ConcurrentFirstCartAndIncrements_SerializeWithoutLostUpdates()
    {
        await using var host = new AuthTestHost();
        await host.InitializeAsync(s => { s.AddScoped<ICartRepository, CartRepository>(); s.AddScoped<IShopRepository, ShopRepository>(); });
        var user = await host.AddUser(); host.Authenticate(user);
        var shop = new Shop(Guid.NewGuid(), "Shop", "cart-shop"); shop.Approve();
        var spu = new Spu(shop.Id, Guid.NewGuid(), "Product", null, null, null, null);
        var sku = new Sku(spu.Id, shop.Id, "SKU", null, 100m, 90m);
        await host.WithDb(async db =>
        {
            db.Shops.Add(shop); db.Spus.Add(spu); db.Skus.Add(sku);
            db.Inventories.Add(new NovaLive.Domain.Inventory.Inventory(sku.Id, shop.Id, 30));
            await db.SaveChangesAsync();
        });
        var results = await Task.WhenAll(Enumerable.Range(0, 12).Select(_ => host.Send(new AddToCartCommand(new(sku.Id, 1)))));
        results.Should().OnlyContain(r => r.IsSuccess);
        await host.WithDb(async db =>
        {
            (await db.Carts.CountAsync(c => c.UserId == user)).Should().Be(1);
            var item = await db.CartItems.SingleAsync(); item.Quantity.Should().Be(12); item.UnitPrice.Should().Be(90m);
        });
    }
    [PostgresFact]
    public async Task ActualCartSkuUniqueRace_RetryAddsWinnerQuantity_AndTransactionRemainsUsable()
    {
        await using var host = new AuthTestHost(); await host.InitializeAsync();
        var user = await host.AddUser(); host.Authenticate(user);
        var shop = new Shop(Guid.NewGuid(), "Shop", "retry-shop"); shop.Approve();
        var spu = new Spu(shop.Id, Guid.NewGuid(), "Product", null, null, null, null);
        var sku = new Sku(spu.Id, shop.Id, "SKU", null, 100m, 90m);
        await host.WithDb(async db =>
        {
            db.Shops.Add(shop); db.Spus.Add(spu); db.Skus.Add(sku);
            db.Inventories.Add(new NovaLive.Domain.Inventory.Inventory(sku.Id, shop.Id, 10));
            db.Carts.Add(new Cart(user, host.Clock.UtcNow)); await db.SaveChangesAsync();
        });
        var arrived = 0; var recovered = 0;
        var ready = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        async Task<Result<CartItemDto>> Add()
        {
            using var scope = host.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var real = new CartRepository(db, db); var forward = new Mock<ICartRepository>();
            // Simulate another writer that does not participate in the advisory lock.
            forward.Setup(c => c.GetByUserIdAsync(user, It.IsAny<CancellationToken>())).Returns((Guid id, CancellationToken ct) => real.GetByUserIdAsync(id, ct));
            forward.Setup(c => c.GetItemsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).Returns((Guid id, CancellationToken ct) => real.GetItemsAsync(id, ct));
            forward.Setup(c => c.AddItemAsync(It.IsAny<CartItem>(), It.IsAny<CancellationToken>())).Returns((CartItem item, CancellationToken ct) => real.AddItemAsync(item, ct));
            var saves = 0;
            forward.Setup(c => c.TrySaveAsync(user, It.IsAny<CancellationToken>())).Returns(async (Guid id, CancellationToken ct) =>
            {
                if (++saves == 1)
                {
                    if (Interlocked.Increment(ref arrived) == 2) ready.SetResult();
                    await ready.Task.WaitAsync(TimeSpan.FromSeconds(20), ct);
                }
                var saved = await real.TrySaveAsync(id, ct);
                if (!saved) Interlocked.Increment(ref recovered);
                return saved;
            });
            var handler = new AddToCartCommandHandler(forward.Object, new SkuRepository(db), new SpuRepository(db),
                new ShopRepository(db), new InventoryRepository(db), host.CurrentUser.Object, host.Clock);
            return await scope.ServiceProvider.GetRequiredService<ITransactionManager>()
                .ExecuteAsync(() => handler.Handle(new(new(sku.Id, 2)), default), false, default);
        }
        (await Task.WhenAll(Add(), Add())).Should().OnlyContain(r => r.IsSuccess);
        recovered.Should().Be(1);
        await host.WithDb(async db => { var item = await db.CartItems.SingleAsync(); item.Quantity.Should().Be(4); });
    }
}
