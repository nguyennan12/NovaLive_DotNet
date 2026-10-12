using System.Collections.Concurrent;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Services;

namespace NovaLive.Infrastructure.Auth;

// Singleton: single-flight is shared by all request scopes in this API process.
// ICacheService is RedisCacheService in production; cached values are permission-code arrays.
public sealed class RolePermissionCache(ICacheService cache) : IPermissionCacheInvalidator
{
    public const string Prefix = "rbac:role-perms:";
    public static readonly TimeSpan Ttl = TimeSpan.FromMinutes(5);
    private readonly ConcurrentDictionary<Guid, SemaphoreSlim> flights = new();
    private readonly SemaphoreSlim publication = new(1, 1);
    private long generation;

    public async Task<string[]> GetAsync(Guid roleId, Func<CancellationToken, Task<string[]>> load,
        CancellationToken cancellationToken = default)
    {
        var key = Prefix + roleId;
        var cached = await cache.GetAsync<string[]>(key, cancellationToken);
        if (cached is not null) return cached;
        var flight = flights.GetOrAdd(roleId, _ => new SemaphoreSlim(1, 1));
        await flight.WaitAsync(cancellationToken);
        try
        {
            while (true)
            {
                var observedGeneration = Interlocked.Read(ref generation);
                var hit = await cache.GetAsync<string[]>(key, cancellationToken);
                if (hit is not null) return hit;

                var codes = (await load(cancellationToken)).Distinct(StringComparer.Ordinal).ToArray();
                await publication.WaitAsync(cancellationToken);
                try
                {
                    // A committed change during the DB read must not repopulate stale permissions.
                    if (observedGeneration != generation) continue;
                    await cache.SetAsync(key, codes, Ttl, cancellationToken);
                    return codes;
                }
                finally { publication.Release(); }
            }
        }
        finally { flight.Release(); }
    }

    public Task InvalidateRoleAsync(Guid roleId, CancellationToken cancellationToken = default) =>
        InvalidateAsync(ct => cache.RemoveAsync(Prefix + roleId, ct), cancellationToken);

    public Task InvalidateAllAsync(CancellationToken cancellationToken = default) =>
        InvalidateAsync(ct => cache.RemoveByPrefixAsync(Prefix, ct), cancellationToken);

    private async Task InvalidateAsync(Func<CancellationToken, Task> remove, CancellationToken cancellationToken)
    {
        await publication.WaitAsync(cancellationToken);
        try
        {
            Interlocked.Increment(ref generation);
            await remove(cancellationToken);
        }
        finally { publication.Release(); }
    }
}
