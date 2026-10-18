using System.Text.Json;
using Microsoft.Extensions.Logging;
using NovaLive.Application.Abstractions.Services;
using StackExchange.Redis;

namespace NovaLive.Infrastructure.Services;

public sealed class RedisCacheService(IConnectionMultiplexer redis, ILogger<RedisCacheService>? logger = null) : ICacheService
{
    private readonly IDatabase _database = redis.GetDatabase();
    private readonly IConnectionMultiplexer _redis = redis;
    private readonly ILogger<RedisCacheService>? _logger = logger;

    public async Task<long> IncrementAsync(string key, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        const string script = "local count = redis.call('INCR', KEYS[1]); if count == 1 then redis.call('PEXPIRE', KEYS[1], ARGV[1]); end; return count;";
        return (long)await _database.ScriptEvaluateAsync(script, [key], [(long)ttl.TotalMilliseconds]).WaitAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_redis.IsConnected) return false;
            return await _database.KeyExistsAsync(key).WaitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Redis ExistsAsync failed for key {Key}.", key);
            return false;
        }
    }

    public async Task<TimeSpan?> GetTimeToLiveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_redis.IsConnected) return null;
            return await _database.KeyTimeToLiveAsync(key).WaitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Redis GetTimeToLiveAsync failed for key {Key}.", key);
            return null;
        }
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_redis.IsConnected)
            {
                _logger?.LogWarning("Redis is not connected. Skipping cache lookup for key {Key}.", key);
                return default;
            }
            var value = await _database.StringGetAsync(key);
            return value.HasValue ? JsonSerializer.Deserialize<T>((string)value!) : default;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Redis GET failed for key {Key}. Falling back to default.", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_redis.IsConnected)
            {
                _logger?.LogWarning("Redis is not connected. Skipping cache write for key {Key}.", key);
                return;
            }
            var payload = JsonSerializer.Serialize(value);
            await _database.StringSetAsync(key, payload, expiry: ttl.HasValue ? (Expiration)ttl.Value : default);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Redis SET failed for key {Key}.", key);
        }
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

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_redis.IsConnected)
            {
                await _database.KeyDeleteAsync(key);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Redis DELETE failed for key {Key}.", key);
        }
    }

    public async Task RemoveByPrefixAsync(string prefixKey, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_redis.IsConnected) return;
            var endpoints = _redis.GetEndPoints();
            foreach (var endpoint in endpoints)
            {
                var server = _redis.GetServer(endpoint);
                if (server.IsReplica) continue;
                cancellationToken.ThrowIfCancellationRequested();
                var keys = server.Keys(database: _database.Database, pattern: $"{prefixKey}*").ToArray();
                if (keys.Length > 0)
                {
                    await _database.KeyDeleteAsync(keys).WaitAsync(cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Redis RemoveByPrefixAsync failed for pattern {Prefix}.", prefixKey);
        }
    }
}
