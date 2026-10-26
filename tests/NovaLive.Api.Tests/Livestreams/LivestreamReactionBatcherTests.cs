using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using NovaLive.Api.Hubs;
using NovaLive.Api.Services;
using NovaLive.Contracts.V1.Livestreams;
using Xunit;

namespace NovaLive.Api.Tests.Livestreams;

public class LivestreamReactionBatcherTests
{
    private readonly Mock<IHubContext<LivestreamHub>> _hubContextMock = new();
    private readonly Mock<IHubClients> _hubClientsMock = new();
    private readonly Mock<IClientProxy> _clientProxyMock = new();
    private readonly Mock<ILogger<LivestreamReactionBatcher>> _loggerMock = new();

    public LivestreamReactionBatcherTests()
    {
        _hubClientsMock.Setup(c => c.Group(It.IsAny<string>()))
            .Returns(_clientProxyMock.Object);
        _hubContextMock.Setup(h => h.Clients)
            .Returns(_hubClientsMock.Object);
    }

    [Fact]
    public async Task FlushBatchAsync_WhenReactionsAccumulated_BroadcastsAggregatedBatchAndClearsAccumulator()
    {
        var batcher = new LivestreamReactionBatcher(_hubContextMock.Object, _loggerMock.Object);
        var sessionId = Guid.NewGuid();

        batcher.AddReaction(sessionId, "heart");
        batcher.AddReaction(sessionId, "HEART");
        batcher.AddReaction(sessionId, "fire");

        LivestreamReactionBatchResponse? sentPayload = null;
        _clientProxyMock.Setup(p => p.SendCoreAsync("ReceiveReaction", It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
            .Callback<string, object?[], CancellationToken>((method, args, ct) =>
            {
                if (args.Length > 0 && args[0] is LivestreamReactionBatchResponse response)
                {
                    sentPayload = response;
                }
            })
            .Returns(Task.CompletedTask);

        await batcher.FlushBatchAsync(CancellationToken.None);

        _hubClientsMock.Verify(c => c.Group($"live_{sessionId:N}"), Times.Once);
        sentPayload.Should().NotBeNull();
        sentPayload!.SessionId.Should().Be(sessionId);
        sentPayload.TotalBatchCount.Should().Be(3);
        sentPayload.Reactions.Should().ContainKey("heart").WhoseValue.Should().Be(2);
        sentPayload.Reactions.Should().ContainKey("fire").WhoseValue.Should().Be(1);

        // Second flush should be empty
        _hubClientsMock.Invocations.Clear();
        await batcher.FlushBatchAsync(CancellationToken.None);
        _hubClientsMock.Verify(c => c.Group(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task FlushBatchAsync_WhenEmpty_DoesNotBroadcast()
    {
        var batcher = new LivestreamReactionBatcher(_hubContextMock.Object, _loggerMock.Object);

        await batcher.FlushBatchAsync(CancellationToken.None);

        _hubClientsMock.Verify(c => c.Group(It.IsAny<string>()), Times.Never);
    }
}
