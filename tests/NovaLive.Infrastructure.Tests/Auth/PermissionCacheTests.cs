using System.Data.Common;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Domain.Rbac;
using NovaLive.Infrastructure.Auth;
using NovaLive.Infrastructure.Persistence;
using NovaLive.Infrastructure.Persistence.Seeding;

namespace NovaLive.Infrastructure.Tests.Auth;

public sealed class PermissionCacheTests
{
    [Fact]
    public async Task MissLoadsDistinctCodes_HitUsesRedisValue_ExpiresInFiveMinutes_AndGuestWorks()
    {
        await using var fixture = await Fixture.Create();
        await using var db = fixture.Db();
        var provider = new PermissionProvider(db, fixture.Cache);
        (await provider.GetPermissionsAsync(["Guest", "Guest"])).Should().BeEquivalentTo("test:read");
        (await provider.GetPermissionsAsync(["Guest"])).Should().BeEquivalentTo("test:read");
        fixture.Reads.Count.Should().Be(1);
        (await fixture.Store.GetTimeToLiveAsync(RolePermissionCache.Prefix + fixture.RoleId)).Should().Be(TimeSpan.FromMinutes(5));
        fixture.Clock.UtcNow += TimeSpan.FromMinutes(5);
        (await provider.GetPermissionsAsync(["Guest"])).Should().BeEquivalentTo("test:read");
        fixture.Reads.Count.Should().Be(2);
    }

    [Theory]
    [InlineData("grant-add")]
    [InlineData("grant-remove")]
    [InlineData("role-update")]
    [InlineData("role-delete")]
    [InlineData("permission-update")]
    [InlineData("permission-delete")]
    public async Task TrackedMutation_InvalidatesAfterSave_AndNextReadSeesCommittedPermissions(string mutation)
    {
        await using var fixture = await Fixture.Create();
        await fixture.Permissions();
        await using (var db = fixture.Db())
        {
            switch (mutation)
            {
                case "grant-add": db.RolePermissions.Add(new RolePermission(fixture.RoleId, fixture.WriteId, fixture.Clock.UtcNow)); break;
                case "grant-remove": db.RolePermissions.Remove(await db.RolePermissions.SingleAsync()); break;
                case "role-update": (await db.Roles.SingleAsync()).UpdateSystemDefinition("Renamed", "updated"); break;
                case "role-delete": db.Roles.Remove(await db.Roles.SingleAsync()); break;
                case "permission-update": db.Entry(await db.Permissions.SingleAsync(p => p.Id == fixture.ReadId)).Property(p => p.Code).CurrentValue = "test:changed"; break;
                case "permission-delete": db.Permissions.Remove(await db.Permissions.SingleAsync(p => p.Id == fixture.ReadId)); break;
            }
            await db.SaveChangesAsync();
        }
        fixture.Invalidator.Calls.Should().Be(1);
        (await fixture.Store.ExistsAsync(RolePermissionCache.Prefix + fixture.RoleId)).Should().BeFalse();
        var expected = mutation switch
        {
            "grant-add" => new[] { "test:read", "test:write" },
            "permission-update" => ["test:changed"],
            "role-update" => ["test:read"],
            _ => Array.Empty<string>()
        };
        (await fixture.Permissions(mutation == "role-update" ? "Renamed" : "Guest")).Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ExplicitTransaction_InvalidatesOnceOnlyAfterCommit(bool synchronous)
    {
        await using var fixture = await Fixture.Create();
        await fixture.Permissions();
        await using var db = fixture.Db();
        await using var transaction = await db.Database.BeginTransactionAsync();
        db.RolePermissions.Remove(await db.RolePermissions.SingleAsync());
        if (synchronous) db.SaveChanges(); else await db.SaveChangesAsync();
        db.RolePermissions.Add(new RolePermission(fixture.RoleId, fixture.WriteId, fixture.Clock.UtcNow));
        if (synchronous) db.SaveChanges(); else await db.SaveChangesAsync();
        fixture.Invalidator.Calls.Should().Be(0);
        (await fixture.Store.GetAsync<string[]>(RolePermissionCache.Prefix + fixture.RoleId)).Should().BeEquivalentTo("test:read");
        // Transactional readers must not return or publish cached, committed values.
        (await new PermissionProvider(db, fixture.Cache).GetPermissionsAsync(["Guest"])).Should().BeEquivalentTo("test:write");
        if (synchronous) transaction.Commit(); else await transaction.CommitAsync();
        fixture.Invalidator.Calls.Should().Be(1);
        (await fixture.Permissions()).Should().BeEquivalentTo("test:write");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RollbackOrDispose_DoesNotInvalidate_OrLeakPendingChangesToNextTransaction(bool disposeOnly)
    {
        await using var fixture = await Fixture.Create();
        await fixture.Permissions();
        await using var db = fixture.Db();
        await using (var transaction = await db.Database.BeginTransactionAsync())
        {
            db.RolePermissions.Remove(await db.RolePermissions.SingleAsync());
            await db.SaveChangesAsync();
            if (!disposeOnly) await transaction.RollbackAsync();
        }
        await using (var empty = await db.Database.BeginTransactionAsync()) await empty.CommitAsync();
        fixture.Invalidator.Calls.Should().Be(0);
        (await fixture.Permissions()).Should().BeEquivalentTo("test:read");
        fixture.Reads.Count.Should().Be(1);
    }

    [Fact]
    public async Task FailedSave_DoesNotInvalidate()
    {
        await using var fixture = await Fixture.Create();
        await fixture.Permissions();
        await using var db = fixture.Db();
        db.Roles.Add(new Role(Guid.NewGuid(), "Guest", fixture.Clock.UtcNow));
        await FluentActions.Awaiting(() => db.SaveChangesAsync()).Should().ThrowAsync<DbUpdateException>();
        fixture.Invalidator.Calls.Should().Be(0);
        (await fixture.Store.ExistsAsync(RolePermissionCache.Prefix + fixture.RoleId)).Should().BeTrue();
    }

    [Fact]
    public async Task ConcurrentMissesAcrossScopes_LoadPermissionQueryOnce()
    {
        await using var fixture = await Fixture.Create();
        fixture.Reads.Pause = true;
        var requests = Enumerable.Range(0, 20).Select(_ => fixture.Permissions()).ToArray();
        await fixture.Reads.Entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
        fixture.Reads.Release.SetResult();
        var results = await Task.WhenAll(requests).WaitAsync(TimeSpan.FromSeconds(10));
        results.Should().OnlyContain(codes => codes.SequenceEqual(new[] { "test:read" }));
        fixture.Reads.Count.Should().Be(1);
    }

    [Fact]
    public async Task InvalidationDuringFill_DoesNotPublishOldSnapshot()
    {
        await using var fixture = await Fixture.Create();
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var calls = 0;
        var fill = fixture.Cache.GetAsync(fixture.RoleId, async _ =>
        {
            if (Interlocked.Increment(ref calls) == 1)
            { entered.SetResult(); await release.Task; return ["stale"]; }
            return ["current"];
        });
        await entered.Task;
        await fixture.Cache.InvalidateRoleAsync(fixture.RoleId);
        release.SetResult();
        (await fill).Should().BeEquivalentTo("current");
        (await fixture.Store.GetAsync<string[]>(RolePermissionCache.Prefix + fixture.RoleId)).Should().BeEquivalentTo("current");
        calls.Should().Be(2);
    }

    [Fact]
    public async Task Seeder_RestoresMissingGrant_AndInvalidatesAllOnceAtCommit()
    {
        await using var fixture = await Fixture.Create();
        await using (var db = fixture.Db())
            await new RbacDataSeeder(db, fixture.Clock, NullLogger<RbacDataSeeder>.Instance).SeedAsync();
        fixture.Invalidator.Calls.Should().Be(1);
        await using (var db = fixture.Db())
        {
            db.RolePermissions.Remove(await db.RolePermissions.FirstAsync(g => g.RoleId == SystemRoleIds.Buyer));
            await db.SaveChangesAsync();
        }
        var before = await fixture.Permissions("Buyer");
        await fixture.Permissions("Seller");
        await fixture.Store.SetAsync("Blacklist:keep", true, TimeSpan.FromMinutes(15));
        fixture.Invalidator.Calls = 0;
        await using (var db = fixture.Db())
            await new RbacDataSeeder(db, fixture.Clock, NullLogger<RbacDataSeeder>.Instance).SeedAsync();
        fixture.Invalidator.Calls.Should().Be(1);
        (await fixture.Store.ExistsAsync(RolePermissionCache.Prefix + SystemRoleIds.Seller)).Should().BeFalse();
        (await fixture.Store.ExistsAsync("Blacklist:keep")).Should().BeTrue();
        (await fixture.Permissions("Buyer")).Should().HaveCount(before.Count + 1);
        fixture.Invalidator.Calls = 0;
        await using (var db = fixture.Db())
            await new RbacDataSeeder(db, fixture.Clock, NullLogger<RbacDataSeeder>.Instance).SeedAsync();
        fixture.Invalidator.Calls.Should().Be(0, "an idempotent seed should not flush the cache");
    }

    private sealed class Fixture : IAsyncDisposable
    {
        private readonly SqliteConnection connection = new("Data Source=:memory:");
        public TestClock Clock { get; } = new();
        public TestCache Store { get; }
        public RolePermissionCache Cache { get; }
        public CountingInvalidator Invalidator { get; }
        public PermissionReads Reads { get; } = new();
        public Guid RoleId { get; } = Guid.NewGuid();
        public Guid ReadId { get; private set; }
        public Guid WriteId { get; private set; }
        private Fixture() { Store = new(Clock); Cache = new(Store); Invalidator = new(Cache); }
        public AppDbContext Db() => new(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection).UseSnakeCaseNamingConvention().AddInterceptors(Reads).Options, Invalidator);
        public static async Task<Fixture> Create()
        {
            var fixture = new Fixture();
            await fixture.connection.OpenAsync();
            await using var db = fixture.Db();
            await db.Database.EnsureCreatedAsync();
            var resource = new Resource("test");
            var read = new Permission(resource.Id, "read", "test:read");
            var write = new Permission(resource.Id, "write", "test:write");
            fixture.ReadId = read.Id; fixture.WriteId = write.Id;
            db.Resources.Add(resource);
            db.Roles.Add(new Role(fixture.RoleId, "Guest", fixture.Clock.UtcNow));
            db.Permissions.AddRange(read, write);
            db.RolePermissions.Add(new RolePermission(fixture.RoleId, read.Id, fixture.Clock.UtcNow));
            await db.SaveChangesAsync();
            fixture.Invalidator.Calls = 0;
            return fixture;
        }
        public async Task<IReadOnlyCollection<string>> Permissions(string role = "Guest")
        {
            await using var db = Db();
            return await new PermissionProvider(db, Cache).GetPermissionsAsync([role]);
        }
        public ValueTask DisposeAsync() => connection.DisposeAsync();
    }

    private sealed class CountingInvalidator(IPermissionCacheInvalidator inner) : IPermissionCacheInvalidator
    {
        public int Calls { get; set; }
        public Task InvalidateRoleAsync(Guid id, CancellationToken ct = default) { Calls++; return inner.InvalidateRoleAsync(id, ct); }
        public Task InvalidateAllAsync(CancellationToken ct = default) { Calls++; return inner.InvalidateAllAsync(ct); }
    }

    private sealed class PermissionReads : DbCommandInterceptor
    {
        public int Count;
        public bool Pause;
        public TaskCompletionSource Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command,
            CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
        {
            if (command.CommandText.Contains("INNER JOIN \"permissions\""))
            {
                Interlocked.Increment(ref Count);
                if (Pause) { Entered.TrySetResult(); await Release.Task.WaitAsync(cancellationToken); }
            }
            return result;
        }
    }
}
