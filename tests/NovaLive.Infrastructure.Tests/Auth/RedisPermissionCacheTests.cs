using System.Net;
using System.Text.Json;
using FluentAssertions;
using Moq;
using NovaLive.Infrastructure.Auth;
using NovaLive.Infrastructure.Services;
using StackExchange.Redis;

namespace NovaLive.Infrastructure.Tests.Auth;

public sealed class RedisPermissionCacheTests
{
    [Fact]
    public async Task RedisAdapter_StoresCodeSetWithTtl_AndDeletesOnlyRbacKeysInConfiguredDatabase()
    {
        var redis = new Mock<IConnectionMultiplexer>();
        var db = new Mock<IDatabase>();
        var server = new Mock<IServer>();
        EndPoint endpoint = new IPEndPoint(IPAddress.Loopback, 6379);
        redis.Setup(r => r.GetDatabase(-1, null)).Returns(db.Object);
        redis.Setup(r => r.GetEndPoints(false)).Returns([endpoint]);
        redis.Setup(r => r.GetServer(endpoint, null)).Returns(server.Object);
        db.SetupGet(d => d.Database).Returns(4);
        var roleId = Guid.NewGuid();
        RedisKey key = RolePermissionCache.Prefix + roleId;
        RedisValue stored = RedisValue.Null;
        Expiration expiry = default;
        db.Setup(d => d.StringGetAsync(key, CommandFlags.None)).ReturnsAsync(() => stored);
        db.Setup(d => d.StringSetAsync(key, It.IsAny<RedisValue>(), It.IsAny<Expiration>(), It.IsAny<ValueCondition>(), CommandFlags.None))
            .Callback((RedisKey k, RedisValue value, Expiration ttl, ValueCondition condition, CommandFlags flags) =>
            { stored = value; expiry = ttl; }).ReturnsAsync(true);
        db.Setup(d => d.KeyDeleteAsync(key, CommandFlags.None)).ReturnsAsync(true);
        server.Setup(s => s.Keys(4, "rbac:role-perms:*", It.IsAny<int>(), 0, 0, CommandFlags.None)).Returns([key]);
        db.Setup(d => d.KeyDeleteAsync(It.IsAny<RedisKey[]>(), CommandFlags.None)).ReturnsAsync(1);

        var cache = new RolePermissionCache(new RedisCacheService(redis.Object));
        (await cache.GetAsync(roleId, _ => Task.FromResult(new[] { "test:read", "test:read" })))
            .Should().BeEquivalentTo("test:read");
        JsonSerializer.Deserialize<string[]>((string)stored!).Should().BeEquivalentTo("test:read");
        expiry.Should().Be((Expiration)TimeSpan.FromMinutes(5));
        (await cache.GetAsync(roleId, _ => throw new InvalidOperationException("Cache hit must not query DB")))
            .Should().BeEquivalentTo("test:read");
        await cache.InvalidateRoleAsync(roleId);
        db.Verify(d => d.KeyDeleteAsync(key, CommandFlags.None), Times.Once);
        await cache.InvalidateAllAsync();
        server.Verify(s => s.Keys(4, "rbac:role-perms:*", It.IsAny<int>(), 0, 0, CommandFlags.None), Times.Once);
        db.Verify(d => d.KeyDeleteAsync(It.Is<RedisKey[]>(keys => keys.Length == 1 && keys[0] == key), CommandFlags.None), Times.Once);
    }
}
