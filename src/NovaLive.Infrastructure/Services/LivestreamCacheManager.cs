using System.Text.Json;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Contracts.V1.Livestreams;
using StackExchange.Redis;

namespace NovaLive.Infrastructure.Services;

public sealed class LivestreamCacheManager(IConnectionMultiplexer redis) : ILivestreamCacheManager
{
    private readonly IDatabase _database = redis.GetDatabase();
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(24);

    public async Task<int> IncrementViewerCountAsync(Guid sessionId, CancellationToken ct = default)
    {
        var key = ViewerKey(sessionId);
        var count = await _database.StringIncrementAsync(key);
        if (count == 1)
        {
            await _database.KeyExpireAsync(key, CacheTtl);
        }

        return (int)count;
    }

    public async Task<int> DecrementViewerCountAsync(Guid sessionId, CancellationToken ct = default)
    {
        var key = ViewerKey(sessionId);
        const string decrementScript = @"
            local current = redis.call('GET', KEYS[1])
            if not current or tonumber(current) <= 0 then
                redis.call('SET', KEYS[1], 0)
                return 0
            else
                return redis.call('DECR', KEYS[1])
            end";

        var result = (long)await _database.ScriptEvaluateAsync(decrementScript, [key]);
        return (int)result;
    }

    public async Task<int> GetViewerCountAsync(Guid sessionId, CancellationToken ct = default)
    {
        var key = ViewerKey(sessionId);
        var value = await _database.StringGetAsync(key);
        return value.HasValue && value.TryParse(out int count) ? Math.Max(0, count) : 0;
    }

    public async Task AddCommentAsync(Guid sessionId, LivestreamCommentResponse comment, CancellationToken ct = default)
    {
        var key = CommentsKey(sessionId);
        var payload = JsonSerializer.Serialize(comment);

        await _database.ListLeftPushAsync(key, payload);
        await _database.ListTrimAsync(key, 0, 99);
        await _database.KeyExpireAsync(key, CacheTtl);
    }

    public async Task<IReadOnlyList<LivestreamCommentResponse>> GetRecentCommentsAsync(
        Guid sessionId,
        int count = 100,
        CancellationToken ct = default)
    {
        var key = CommentsKey(sessionId);
        var values = await _database.ListRangeAsync(key, 0, count - 1);
        if (values.Length == 0)
        {
            return [];
        }

        var results = new List<LivestreamCommentResponse>(values.Length);
        foreach (var val in values)
        {
            if (!val.HasValue)
            {
                continue;
            }

            var item = JsonSerializer.Deserialize<LivestreamCommentResponse>((string)val!);
            if (item is not null)
            {
                results.Add(item);
            }
        }

        return results;
    }

    private static string ViewerKey(Guid sessionId) => $"livestream:{sessionId:N}:viewers";
    private static string CommentsKey(Guid sessionId) => $"livestream:{sessionId:N}:comments";
}
