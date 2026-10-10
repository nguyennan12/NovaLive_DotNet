using System.Text.Json;
using NovaLive.Application.Abstractions.Services;
using StackExchange.Redis;

namespace NovaLive.Infrastructure.Services;

public sealed class RedisCacheService(IConnectionMultiplexer redis) : ICacheService
{
    private readonly IDatabase _database = redis.GetDatabase();
    private readonly IConnectionMultiplexer _redis = redis;

    public async Task<long> IncrementAsync(string key, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        const string script = "local count = redis.call('INCR', KEYS[1]); if count == 1 then redis.call('PEXPIRE', KEYS[1], ARGV[1]); end; return count;";
        return (long)await _database.ScriptEvaluateAsync(script, [key], [(long)ttl.TotalMilliseconds]).WaitAsync(cancellationToken);
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default) =>
        _database.KeyExistsAsync(key).WaitAsync(cancellationToken);

    public Task<TimeSpan?> GetTimeToLiveAsync(string key, CancellationToken cancellationToken = default) =>
        _database.KeyTimeToLiveAsync(key).WaitAsync(cancellationToken);

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var value = await _database.StringGetAsync(key);
        return value.HasValue ? JsonSerializer.Deserialize<T>((string)value!) : default;
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Serialize(value);
        return _database.StringSetAsync(key, payload, expiry: ttl.HasValue ? (Expiration)ttl.Value : default);
    }

    public async Task<T> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? ttl = null,
        CancellationToken cancellationToken = default)
    {
        var cached = await GetAsync<T>(key, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        var value = await factory(cancellationToken);
        if (value is not null)
        {
            await SetAsync(key, value, ttl, cancellationToken);
        }

        return value;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        return _database.KeyDeleteAsync(key);
    }

    public async Task RemoveByPrefixAsync(string prefixKey, CancellationToken cancellationToken = default)
    {
        var endpoints = _redis.GetEndPoints();
        foreach (var endpoint in endpoints)
        {
            var server = _redis.GetServer(endpoint);
            var keys = server.Keys(pattern: $"{prefixKey}*").ToArray();
            if (keys.Length > 0)
            {
                await _database.KeyDeleteAsync(keys);
            }
        }
    }
}
