using System.Text.Json;
using NovaLive.Application.Abstractions.Cache;
using NovaLive.Application.Abstractions.Idempotency;

namespace NovaLive.Infrastructure.Idempotency;

public sealed class RedisIdempotencyService(ICacheService cacheService) : IIdempotencyService
{
    private const string KeyPrefix = "idempotency:";

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        var cached = await cacheService.GetAsync<string>($"{KeyPrefix}{key}", cancellationToken);
        return cached is not null;
    }

    public async Task CreateAsync(string key, string requestName, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
    {
        await cacheService.SetAsync(
            $"{KeyPrefix}{key}",
            requestName,
            ttl ?? TimeSpan.FromHours(24),
            cancellationToken);
    }

    public async Task<TResponse?> GetResponseAsync<TResponse>(string key, CancellationToken cancellationToken = default)
    {
        return await cacheService.GetAsync<TResponse>($"{KeyPrefix}response:{key}", cancellationToken);
    }

    public async Task SaveResponseAsync<TResponse>(string key, TResponse response, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
    {
        await cacheService.SetAsync(
            $"{KeyPrefix}response:{key}",
            response,
            ttl ?? TimeSpan.FromHours(24),
            cancellationToken);
    }
}
