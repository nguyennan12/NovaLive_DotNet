namespace NovaLive.Application.Abstractions.Services;

public interface IIdempotencyService
{
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    Task CreateAsync(string key, string requestName, TimeSpan? ttl = null, CancellationToken cancellationToken = default);

    Task<TResponse?> GetResponseAsync<TResponse>(string key, CancellationToken cancellationToken = default);

    Task SaveResponseAsync<TResponse>(string key, TResponse response, TimeSpan? ttl = null, CancellationToken cancellationToken = default);
}
