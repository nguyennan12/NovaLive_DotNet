using NovaLive.Infrastructure.Persistence.Repositories;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Npgsql;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.UseCases.Shops;
using NovaLive.Application.UseCases.Shops.Commands.RegisterShop;
using NovaLive.Application.UseCases.Shops.Commands.FollowShop;
using NovaLive.Application.UseCases.Shops.Commands.AddShopAddress;
using NovaLive.Contracts.V1.Shops;
using NovaLive.Domain.Common;
using NovaLive.Domain.Shops;
using NovaLive.Domain.Users;
using NovaLive.Infrastructure.Persistence;
using NovaLive.Infrastructure.Tests.Auth;
namespace NovaLive.Infrastructure.Tests.Shops;

public sealed class PostgresShopConcurrencyTests
{
    [PostgresFact]
    public async Task ConcurrentRegistration_OneWins_OtherGetsOwner400_WithOneKyc()
    {
        await using var fixture = await DatabaseFixture.Create();
        var user = new User("owner@example.com", "hash", "Owner", "0912345678"); user.Activate();
        await using (var db = fixture.Context()) { db.Users.Add(user); await db.SaveChangesAsync(); }
        var barrier = new SaveBarrier();
        async Task<Result<ShopResponse>> Register()
        {
            await using var db = fixture.Context(); var real = new ShopRepository(db);
            var concurrency = new Mock<IShopRepository>();
            concurrency.Setup(p => p.SaveRegistrationAsync(It.IsAny<CancellationToken>())).Returns(async (CancellationToken ct) =>
                { await barrier.Wait(); return await real.SaveRegistrationAsync(ct); });
            var handler = new RegisterShopCommandHandler(db, Current(user.Id), concurrency.Object);
            return await new TransactionManager(db, new AfterCommitActions()).ExecuteAsync(() => handler.Handle(new(new(
                "Shop", "Description", "0912345678", "shop@example.com", "https://cdn.example/kyc-docs/front.png",
                "https://cdn.example/kyc-docs/back.png", "12345", "Bank", Slug: Guid.NewGuid().ToString())), default), false, default);
        }
        var results = await Task.WhenAll(Register(), Register());
        results.Should().ContainSingle(r => r.IsSuccess);
        results.Single(r => r.IsFailure).Error.Should().Be(ShopErrors.AlreadyOwned);
        await using var check = fixture.Context();
        (await check.Shops.CountAsync()).Should().Be(1); (await check.ShopVerifications.CountAsync()).Should().Be(1);
        (await check.ShopWallets.CountAsync()).Should().Be(0);
    }

    [PostgresFact]
    public async Task ConcurrentFollow_23505IsIdempotent_AndTransactionStillCommits()
    {
        await using var fixture = await DatabaseFixture.Create();
        var shop = new Shop(Guid.NewGuid(), "Shop", "shop"); shop.Approve();
        await using (var db = fixture.Context()) { db.Shops.Add(shop); await db.SaveChangesAsync(); }
        var barrier = new SaveBarrier(); var userId = Guid.NewGuid();
        async Task<Result> Follow()
        {
            await using var db = fixture.Context(); var real = new ShopRepository(db);
            var concurrency = new Mock<IShopRepository>();
            concurrency.Setup(p => p.SaveFollowAsync(It.IsAny<CancellationToken>())).Returns(async (CancellationToken ct) =>
                { await barrier.Wait(); await real.SaveFollowAsync(ct); });
            return await new TransactionManager(db, new AfterCommitActions()).ExecuteAsync(() =>
                new FollowShopCommandHandler(db, Current(userId), concurrency.Object).Handle(new(shop.Id), default), false, default);
        }
        (await Task.WhenAll(Follow(), Follow())).Should().OnlyContain(r => r.IsSuccess);
        await using var check = fixture.Context(); (await check.ShopFollowers.CountAsync()).Should().Be(1);
    }

    [PostgresFact]
    public async Task ConcurrentFirstWarehouses_SerializeAndKeepExactlyOnePrimaryAndReturn()
    {
        await using var fixture = await DatabaseFixture.Create(); var userId = Guid.NewGuid();
        await using (var db = fixture.Context()) { var shop = new Shop(userId, "Shop", "shop"); shop.Approve(); db.Shops.Add(shop); await db.SaveChangesAsync(); }
        async Task<Result<ShopAddressResponse>> Add()
        {
            await using var db = fixture.Context();
            var access = new ShopAccess(db, Current(userId), new ShopRepository(db));
            var handler = new AddShopAddressCommandHandler(db, access, new ShopAddressDefaults(db));
            return await new TransactionManager(db, new AfterCommitActions()).ExecuteAsync(() => handler.Handle(new(new(
                "Warehouse", "Contact", "0912345678", "P", "D", "W", "Street", false, false, 1, 2, "3")), default), false, default);
        }
        (await Task.WhenAll(Add(), Add())).Should().OnlyContain(r => r.IsSuccess);
        await using var check = fixture.Context(); var addresses = await check.ShopAddresses.ToListAsync();
        addresses.Should().HaveCount(2).And.ContainSingle(a => a.IsPrimary).And.ContainSingle(a => a.IsReturn);
    }

    private static ICurrentUser Current(Guid id)
    {
        var user = new Mock<ICurrentUser>(); user.SetupGet(u => u.IsAuthenticated).Returns(true);
        user.SetupGet(u => u.UserId).Returns(id); return user.Object;
    }
    private sealed class SaveBarrier
    {
        private int arrived;
        private readonly TaskCompletionSource ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public async Task Wait()
        {
            if (Interlocked.Increment(ref arrived) == 2) ready.SetResult();
            await ready.Task.WaitAsync(TimeSpan.FromSeconds(20));
        }
    }
    private sealed class DatabaseFixture(string adminConnection, string name, string connectionString) : IAsyncDisposable
    {
        public AppDbContext Context() => new(new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString).UseSnakeCaseNamingConvention().Options);
        public static async Task<DatabaseFixture> Create()
        {
            var adminConnection = Environment.GetEnvironmentVariable("NOVALIVE_TEST_POSTGRES")!;
            var name = "novalive_shop_test_" + Guid.NewGuid().ToString("N");
            await using var admin = new NpgsqlConnection(adminConnection); await admin.OpenAsync();
            await using var command = new NpgsqlCommand($"CREATE DATABASE {name}", admin); await command.ExecuteNonQueryAsync();
            var fixture = new DatabaseFixture(adminConnection, name,
                new NpgsqlConnectionStringBuilder(adminConnection) { Database = name, Pooling = false }.ConnectionString);
            try
            {
                // Provider/concurrency test against the actual current model, independent of historical migration drift.
                await using var db = fixture.Context(); await db.Database.EnsureCreatedAsync(); return fixture;
            }
            catch { await fixture.DisposeAsync(); throw; }
        }
        public async ValueTask DisposeAsync()
        {
            // Only the uniquely named disposable database created above is removed.
            await using var admin = new NpgsqlConnection(adminConnection); await admin.OpenAsync();
            await using var command = new NpgsqlCommand($"DROP DATABASE {name} WITH (FORCE)", admin);
            await command.ExecuteNonQueryAsync();
        }
    }
}
