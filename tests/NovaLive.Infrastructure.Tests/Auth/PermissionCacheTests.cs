using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Domain.Common;
using NovaLive.Domain.Rbac;
using NovaLive.Domain.Users;
using NovaLive.Infrastructure.Persistence;
using NovaLive.Infrastructure.Persistence.Repositories;

namespace NovaLive.Infrastructure.Tests.Auth;

public sealed class PermissionCacheTests
{
    [Fact]
    public async Task PermissionRepository_LoadsDistinctCodesForUser()
    {
        await using var fixture = await Fixture.Create();
        await using var db = fixture.Db();
        var repo = new PermissionRepository(db);

        var codes = await repo.GetPermissionCodesAsync(fixture.UserId, ["Guest"]);
        codes.Should().BeEquivalentTo("test:read");
    }

    [Fact]
    public async Task UserRepository_LoadsUserById()
    {
        await using var fixture = await Fixture.Create();
        await using var db = fixture.Db();
        var repo = new UserRepository(db);

        var user = await repo.GetByIdAsync(fixture.UserId);
        user.Should().NotBeNull();
        user!.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task PermissionCacheFlow_MissSetsCache_HitReadsCache_InvalidateRemoves()
    {
        await using var fixture = await Fixture.Create();
        await using var db = fixture.Db();
        var permRepo = new PermissionRepository(db);
        var cache = fixture.Store;
        var cacheKey = $"perm:{fixture.UserId}:Guest";

        // 1. Initial: Cache Miss
        (await cache.GetAsync<string[]>(cacheKey)).Should().BeNull();

        // 2. Load and set cache
        var codes = await permRepo.GetPermissionCodesAsync(fixture.UserId, ["Guest"]);
        await cache.SetAsync(cacheKey, codes, TimeSpan.FromMinutes(5));

        // 3. Cache Hit
        var cached = await cache.GetAsync<string[]>(cacheKey);
        cached.Should().BeEquivalentTo("test:read");
        (await cache.GetTimeToLiveAsync(cacheKey)).Should().Be(TimeSpan.FromMinutes(5));

        // 4. Invalidation
        await cache.RemoveAsync(cacheKey);
        (await cache.GetAsync<string[]>(cacheKey)).Should().BeNull();
    }

    private sealed class Fixture : IAsyncDisposable
    {
        private readonly SqliteConnection connection = new("Data Source=:memory:");
        public TestClock Clock { get; } = new();
        public TestCache Store { get; }
        public Guid RoleId { get; } = Guid.NewGuid();
        public Guid UserId { get; } = Guid.NewGuid();

        private Fixture() { Store = new(Clock); }

        public AppDbContext Db() => new(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection).UseSnakeCaseNamingConvention().Options);

        public static async Task<Fixture> Create()
        {
            var fixture = new Fixture();
            await fixture.connection.OpenAsync();
            await using var db = fixture.Db();
            await db.Database.EnsureCreatedAsync();

            var resource = new Resource("test");
            var read = new Permission(resource.Id, "read", "test:read");
            var write = new Permission(resource.Id, "write", "test:write");
            var role = new Role(fixture.RoleId, "Guest", fixture.Clock.UtcNow);
            var user = new User("test@example.com", "hash", "Test User", "0912345678");
            user.Activate();
            typeof(User).GetProperty(nameof(User.Id))!.SetValue(user, fixture.UserId);

            db.Resources.Add(resource);
            db.Roles.Add(role);
            db.Permissions.AddRange(read, write);
            db.RolePermissions.Add(new RolePermission(fixture.RoleId, read.Id, fixture.Clock.UtcNow));
            db.Users.Add(user);
            db.UserRoles.Add(new UserRole(fixture.UserId, fixture.RoleId, fixture.Clock.UtcNow));
            await db.SaveChangesAsync();

            return fixture;
        }

        public ValueTask DisposeAsync() => connection.DisposeAsync();
    }
}
