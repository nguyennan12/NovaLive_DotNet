using NovaLive.Application.Abstractions.Services;
using NovaLive.Domain.Common;

namespace NovaLive.Infrastructure.Services;

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
        // A non-generic Result has no JSON constructor. Store its successful acknowledgement as a marker.
        if (typeof(TResponse) == typeof(Result))
        {
            var acknowledged = await cacheService.GetAsync<bool?>($"{KeyPrefix}response:{key}", cancellationToken);
            return acknowledged == true ? (TResponse)(object)Result.Success() : default;
        }
        return await cacheService.GetAsync<TResponse>($"{KeyPrefix}response:{key}", cancellationToken);
    }

    public async Task SaveResponseAsync<TResponse>(string key, TResponse response, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
    {
        if (typeof(TResponse) == typeof(Result) && response is Result result)
        {
            if (result.IsSuccess)
                await cacheService.SetAsync($"{KeyPrefix}response:{key}", true, ttl ?? TimeSpan.FromHours(24), cancellationToken);
            return;
        }
        await cacheService.SetAsync(
            $"{KeyPrefix}response:{key}",
            response,
            ttl ?? TimeSpan.FromHours(24),
            cancellationToken);
    }
}
