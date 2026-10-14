using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NovaLive.Infrastructure.Tests.Auth;
using NovaLive.Domain.Carts;
using NovaLive.Domain.Products;
using NovaLive.Infrastructure.Persistence;
using NovaLive.Infrastructure.Persistence.Repositories;
using NovaLive.Infrastructure.Persistence.Migrations;
namespace NovaLive.Infrastructure.Tests.Carts;
public sealed class CartRepositoryTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 14, 0, 0, 0, TimeSpan.Zero);
    [Fact]
    public async Task ReadsAreScoped_WritesRollback_AndHardDeletePersists()
    {
        using var connection = new SqliteConnection("Data Source=:memory:"); await connection.OpenAsync();
        await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).ReplaceService<IModelCustomizer, SqliteTimeModelCustomizer>().UseSnakeCaseNamingConvention().Options);
        await db.Database.EnsureCreatedAsync(); var repo = new CartRepository(db, db);
        var cart = new Cart(Guid.NewGuid(), Now); var other = new Cart(Guid.NewGuid(), Now);
        var item = cart.AddItem([], Guid.NewGuid(), Guid.NewGuid(), 2, 10m, Now).Value!;
        var foreign = other.AddItem([], Guid.NewGuid(), Guid.NewGuid(), 1, 20m, Now).Value!;
        await repo.AddAsync(cart); await repo.AddAsync(other); await repo.AddItemAsync(item); await repo.AddItemAsync(foreign);
        (await repo.TrySaveAsync(cart.UserId)).Should().BeTrue(); db.ChangeTracker.Clear();
        (await repo.GetCartItemsByIdsAsync([item.Id, foreign.Id], cart.UserId)).Should().ContainSingle().Which.Id.Should().Be(item.Id);
        (await repo.GetByUserIdAsync(Guid.NewGuid())).Should().BeNull();
        (await repo.GetItemsAsync(cart.Id)).Should().ContainSingle().Which.UpdatedAt.Should().Be(Now);
        await using (var tx = await db.Database.BeginTransactionAsync())
        {
            repo.RemoveItems(await repo.GetItemsAsync(cart.Id)); await db.SaveChangesAsync();
            await tx.RollbackAsync(); db.ChangeTracker.Clear();
        }
        (await repo.GetItemsAsync(cart.Id)).Should().HaveCount(1);
        repo.RemoveItems(await repo.GetItemsAsync(cart.Id)); await db.SaveChangesAsync(); db.ChangeTracker.Clear();
        (await repo.GetItemsAsync(cart.Id)).Should().BeEmpty();
        (await repo.GetItemsAsync(other.Id)).Should().HaveCount(1);
    }
    [Theory]
    [InlineData("ix_cart_items_cart_id_sku_id")] [InlineData("ix_carts_user_id")]
    public async Task UniqueRace_DetachesOnlyCartState_SoCallerCanReloadAndIncrement(string constraint)
    {
        using var connection = new SqliteConnection("Data Source=:memory:"); await connection.OpenAsync();
        var failure = new CartSaveFailureInterceptor(constraint);
        await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection).ReplaceService<IModelCustomizer, SqliteTimeModelCustomizer>().UseSnakeCaseNamingConvention().AddInterceptors(failure).Options);
        await db.Database.EnsureCreatedAsync();
        var user = Guid.NewGuid(); var winner = new Cart(user, Now);
        var item = winner.AddItem([], Guid.NewGuid(), Guid.NewGuid(), 3, 10m, Now).Value!;
        db.Carts.Add(winner); db.CartItems.Add(item); await db.SaveChangesAsync(); db.ChangeTracker.Clear();
        var catalog = new Sku(Guid.NewGuid(), Guid.NewGuid(), "SKU", null, 10m, 10m); db.Skus.Attach(catalog);
        await using var tx = await db.Database.BeginTransactionAsync();
        var repo = new CartRepository(db, db);
        var loser = constraint == "ix_carts_user_id" ? new Cart(user, Now) : (await repo.GetByUserIdAsync(user))!;
        if (constraint == "ix_carts_user_id") await repo.AddAsync(loser);
        await repo.AddItemAsync(new CartItem(Guid.NewGuid(), loser.Id, item.SkuId, item.ShopId, 2, 10m, Now));
        failure.Enabled = true;
        (await repo.TrySaveAsync(user)).Should().BeFalse();
        db.ChangeTracker.Entries<Cart>().Should().BeEmpty(); db.ChangeTracker.Entries<CartItem>().Should().BeEmpty();
        db.Entry(catalog).State.Should().Be(EntityState.Unchanged);
        var reloaded = (await repo.GetByUserIdAsync(user))!;
        var items = await repo.GetItemsAsync(reloaded.Id);
        reloaded.AddItem(items, item.SkuId, item.ShopId, 2, 99m, Now).Value!.Quantity.Should().Be(5);
        (await repo.TrySaveAsync(user)).Should().BeTrue(); await tx.CommitAsync(); db.ChangeTracker.Clear();
        (await db.CartItems.SingleAsync()).Quantity.Should().Be(5);
        (await db.CartItems.SingleAsync()).UnitPrice.Should().Be(10m);
    }
    [Fact]
    public async Task UnrelatedUniqueViolation_Propagates()
    {
        using var connection = new SqliteConnection("Data Source=:memory:"); await connection.OpenAsync();
        var failure = new CartSaveFailureInterceptor("unrelated_index") { Enabled = true };
        await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).AddInterceptors(failure).Options);
        var save = () => new CartRepository(db, db).TrySaveAsync(Guid.NewGuid());
        await save.Should().ThrowAsync<DbUpdateException>();
    }
    [Fact]
    public void MigrationSnapshotMatchesModel_AndRestorationPreservesHistoricalColumn()
    {
        using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=model_only;Username=test;Password=test").UseSnakeCaseNamingConvention().Options);
        db.Database.HasPendingModelChanges().Should().BeFalse();
        var migration = new RestoreCartItemUpdatedAt();
        migration.UpOperations.Should().ContainSingle();
        var sql = ((Microsoft.EntityFrameworkCore.Migrations.Operations.SqlOperation)migration.UpOperations[0]).Sql;
        sql.Should().Contain("ADD COLUMN IF NOT EXISTS").And.Contain("SET updated_at = added_at");
        migration.DownOperations.Should().BeEmpty();
    }
}
