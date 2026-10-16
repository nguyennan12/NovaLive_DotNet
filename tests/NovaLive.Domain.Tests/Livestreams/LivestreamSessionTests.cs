using FluentAssertions;
using NovaLive.Domain.Common;
using NovaLive.Domain.Livestreams;
using Xunit;

namespace NovaLive.Domain.Tests.Livestreams;

public class LivestreamSessionTests
{
    private readonly Guid _shopId = Guid.NewGuid();
    private readonly Guid _hostId = Guid.NewGuid();

    [Fact]
    public void CreateLive_ValidInput_InitializesInLiveState()
    {
        var now = DateTimeOffset.UtcNow;
        var channelName = $"live_{Guid.NewGuid():N}";

        var session = LivestreamSession.CreateLive(
            _shopId,
            _hostId,
            "Phiên Live Khuyến Mãi Đầu Năm",
            "https://cdn.novalive.vn/banner.jpg",
            channelName,
            now);

        session.Should().NotBeNull();
        session.ShopId.Should().Be(_shopId);
        session.HostId.Should().Be(_hostId);
        session.Status.Should().Be(LivestreamStatus.Live);
        session.StartedAt.Should().Be(now);
        session.AgoraChannelName.Should().Be(channelName);
        session.ViewerCount.Should().Be(0);
        session.PeakViewerCount.Should().Be(0);
    }

    [Fact]
    public void End_WhenLive_TransitionsToEndedSuccessfully()
    {
        var now = DateTimeOffset.UtcNow;
        var session = LivestreamSession.CreateLive(
            _shopId,
            _hostId,
            "Phiên Live",
            null,
            $"live_{Guid.NewGuid():N}",
            now);

        var endNow = now.AddHours(1);
        var result = session.End(endNow);

        result.IsSuccess.Should().BeTrue();
        session.Status.Should().Be(LivestreamStatus.Ended);
        session.EndedAt.Should().Be(endNow);
    }

    [Fact]
    public void End_WhenAlreadyEnded_ReturnsSuccessIdempotently()
    {
        var now = DateTimeOffset.UtcNow;
        var session = LivestreamSession.CreateLive(
            _shopId,
            _hostId,
            "Phiên Live",
            null,
            $"live_{Guid.NewGuid():N}",
            now);

        session.End(now.AddMinutes(30));
        var secondEnd = session.End(now.AddMinutes(40));

        secondEnd.IsSuccess.Should().BeTrue();
        session.Status.Should().Be(LivestreamStatus.Ended);
    }

    [Fact]
    public void Start_WhenAlreadyEnded_ReturnsConflictError()
    {
        var now = DateTimeOffset.UtcNow;
        var session = LivestreamSession.CreateLive(
            _shopId,
            _hostId,
            "Phiên Live",
            null,
            $"live_{Guid.NewGuid():N}",
            now);

        session.End(now.AddMinutes(30));
        var startResult = session.Start(now.AddMinutes(35));

        startResult.IsFailure.Should().BeTrue();
        startResult.Error.Code.Should().Be("Livestream.InvalidState");
    }

    [Fact]
    public void UpdateViewerStats_WhenHigherThanCurrentPeak_UpdatesPeak()
    {
        var now = DateTimeOffset.UtcNow;
        var session = LivestreamSession.CreateLive(
            _shopId,
            _hostId,
            "Phiên Live",
            null,
            $"live_{Guid.NewGuid():N}",
            now);

        session.UpdateViewerStats(150);
        session.ViewerCount.Should().Be(150);
        session.PeakViewerCount.Should().Be(150);

        session.UpdateViewerStats(120);
        session.ViewerCount.Should().Be(120);
        session.PeakViewerCount.Should().Be(150);

        session.UpdateViewerStats(200);
        session.ViewerCount.Should().Be(200);
        session.PeakViewerCount.Should().Be(200);
    }
}
