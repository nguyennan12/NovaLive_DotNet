using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using NovaLive.Api.Hubs;
using NovaLive.Contracts.V1.Livestreams;

namespace NovaLive.Api.Services;

public sealed class LivestreamReactionBatcher(
    IHubContext<LivestreamHub> hubContext,
    ILogger<LivestreamReactionBatcher> logger)
    : BackgroundService, ILivestreamReactionBatcher
{
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<string, int>> _accumulators = new();

    public void AddReaction(Guid sessionId, string reactionType)
    {
        if (sessionId == Guid.Empty)
        {
            return;
        }

        var type = string.IsNullOrWhiteSpace(reactionType) ? "heart" : reactionType.Trim().ToLowerInvariant();
        if (type.Length > 20)
        {
            type = type[..20];
        }

        var sessionDict = _accumulators.GetOrAdd(sessionId, _ => new ConcurrentDictionary<string, int>());
        if (sessionDict.Count < 20 || sessionDict.ContainsKey(type))
        {
            sessionDict.AddOrUpdate(type, 1, (_, count) => count + 1);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(500));

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            await FlushBatchAsync(stoppingToken);
        }
    }

    public async Task FlushBatchAsync(CancellationToken ct = default)
    {
        if (_accumulators.IsEmpty)
        {
            return;
        }

        var sessions = _accumulators.Keys.ToList();
        foreach (var sessionId in sessions)
        {
            if (!_accumulators.TryRemove(sessionId, out var reactionsDict) || reactionsDict.IsEmpty)
            {
                continue;
            }

            var dict = reactionsDict.ToDictionary(k => k.Key, v => v.Value);
            var total = dict.Values.Sum();
            if (total <= 0)
            {
                continue;
            }

            var batchResponse = new LivestreamReactionBatchResponse(
                sessionId,
                dict,
                total,
                DateTimeOffset.UtcNow);

            try
            {
                await hubContext.Clients
                    .Group($"live_{sessionId:N}")
                    .SendAsync("ReceiveReaction", batchResponse, ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogWarning(ex, "Failed to broadcast reaction batch for session {SessionId}.", sessionId);
            }
        }
    }
}
