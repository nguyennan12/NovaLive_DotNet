using NovaLive.Infrastructure.Persistence.Repositories;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql;
using NovaLive.Application.UseCases.Shops;
using NovaLive.Domain.Common;
using NovaLive.Domain.Shops;
using NovaLive.Infrastructure.Persistence;
namespace NovaLive.Infrastructure.Tests.Shops;

public sealed class ShopConcurrencyServiceTests
{
    [Fact]
    public void MigrationSnapshot_MatchesCurrentPostgresModel_AndOnlyAddsShopIndexes()
    {
        using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=model_only;Username=test;Password=test")
            .UseSnakeCaseNamingConvention().Options);
        db.Database.HasPendingModelChanges().Should().BeFalse();
        var migration = new NovaLive.Infrastructure.Persistence.Migrations.AddShopAddressDefaultIndexes();
        migration.UpOperations.Should().HaveCount(2).And.OnlyContain(operation =>
            operation is Microsoft.EntityFrameworkCore.Migrations.Operations.CreateIndexOperation);
        migration.UpOperations.Cast<Microsoft.EntityFrameworkCore.Migrations.Operations.CreateIndexOperation>()
            .Should().OnlyContain(index => index.Table == "shop_addresses" && index.IsUnique);
    }

    [Theory]
    [InlineData("ix_shops_owner_id", ErrorType.Validation)]
    [InlineData("ix_shops_slug", ErrorType.AlreadyExists)]
    public async Task PostgreSql23505_MapsSpecificRegistrationConstraints_AndRollsBack(string constraint, ErrorType type)
    {
        using var connection = new SqliteConnection("Data Source=:memory:"); await connection.OpenAsync();
        var failure = new FailNextSave(constraint);
        await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection).UseSnakeCaseNamingConvention().AddInterceptors(failure).Options);
        await db.Database.EnsureCreatedAsync();
        var transaction = new TransactionManager(db, new AfterCommitActions());
        var result = await transaction.ExecuteAsync(async () =>
        {
            db.Shops.Add(new Shop(Guid.NewGuid(), "Shop", "shop"));
            // Save an earlier write to prove the Result failure rolls back the entire transaction.
            await db.SaveChangesAsync();
            db.ShopVerifications.Add(new ShopVerification(Guid.NewGuid(), Guid.NewGuid(), "front", "back", null, "12345", "Bank", null));
            failure.Enabled = true;
            var error = await new ShopRepository(db).SaveRegistrationAsync(default);
            return error is null ? Result.Success() : Result.Failure(error);
        }, false, default);
        result.Error.Type.Should().Be(type);
        if (constraint == "ix_shops_owner_id") result.Error.Should().Be(ShopErrors.AlreadyOwned);
        (await db.Shops.CountAsync()).Should().Be(0);
        (await db.ShopVerifications.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task BothOwnerAndSlugCollide_PrioritizesOwner400EvenWhenPostgresReportsSlug()
    {
        using var connection = new SqliteConnection("Data Source=:memory:"); await connection.OpenAsync();
        var failure = new FailNextSave("ix_shops_slug");
        await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection).UseSnakeCaseNamingConvention().AddInterceptors(failure).Options);
        await db.Database.EnsureCreatedAsync(); var owner = Guid.NewGuid();
        db.Shops.Add(new Shop(owner, "Shop", "shop")); await db.SaveChangesAsync();
        db.Shops.Add(new Shop(owner, "Shop", "shop")); failure.Enabled = true;
        (await new ShopRepository(db).SaveRegistrationAsync(default)).Should().Be(ShopErrors.AlreadyOwned);
    }

    [Fact]
    public async Task Follow23505_DetachesLosingInsert_FinalTransactionSaveSucceeds()
    {
        using var connection = new SqliteConnection("Data Source=:memory:"); await connection.OpenAsync();
        var failure = new FailNextSave("ix_shop_followers_shop_id_user_id");
        await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection).UseSnakeCaseNamingConvention().AddInterceptors(failure).Options);
        await db.Database.EnsureCreatedAsync();
        var shop = Guid.NewGuid(); var user = Guid.NewGuid();
        db.ShopFollowers.Add(new ShopFollower(shop, user)); await db.SaveChangesAsync();
        var result = await new TransactionManager(db, new AfterCommitActions()).ExecuteAsync(async () =>
        {
            db.ShopFollowers.Add(new ShopFollower(shop, user)); failure.Enabled = true;
            await new ShopRepository(db).SaveFollowAsync(default); return Result.Success();
        }, false, default);
        result.IsSuccess.Should().BeTrue();
        (await db.ShopFollowers.CountAsync()).Should().Be(1);
        db.ChangeTracker.Entries<ShopFollower>().Should().NotContain(e => e.State == EntityState.Added);
    }

    [Fact]
    public async Task UnrelatedUniqueViolation_IsNotSwallowed()
    {
        using var connection = new SqliteConnection("Data Source=:memory:"); await connection.OpenAsync();
        var failure = new FailNextSave("unrelated_constraint") { Enabled = true };
        await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection).AddInterceptors(failure).Options);
        var action = () => new ShopRepository(db).SaveRegistrationAsync(default);
        await action.Should().ThrowAsync<DbUpdateException>();
    }

    [Theory]
    [InlineData(true)] [InlineData(false)]
    public async Task Database_EnforcesOneDefaultPerShop(bool primary)
    {
        using var connection = new SqliteConnection("Data Source=:memory:"); await connection.OpenAsync();
        await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection).UseSnakeCaseNamingConvention().Options);
        await db.Database.EnsureCreatedAsync(); var shopId = Guid.NewGuid();
        for (var i = 0; i < 2; i++)
        {
            var address = new ShopAddress(shopId, "W", "C", "0912345678", 1, "P", 2, "D", "3", "W", "Street");
            address.SetPrimary(primary); address.SetReturn(!primary); db.ShopAddresses.Add(address);
        }
        var action = () => db.SaveChangesAsync(); await action.Should().ThrowAsync<DbUpdateException>();
    }

    private sealed class FailNextSave(string constraint) : SaveChangesInterceptor
    {
        public bool Enabled { get; set; }
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData data,
            InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (!Enabled) return ValueTask.FromResult(result);
            Enabled = false;
            throw new DbUpdateException("Simulated provider race", new PostgresException("Duplicate", "ERROR", "ERROR", "23505", constraintName: constraint));
        }
    }
}
