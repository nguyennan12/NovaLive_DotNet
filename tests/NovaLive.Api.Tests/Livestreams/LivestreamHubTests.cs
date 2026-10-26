using System.Security.Claims;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Moq;
using NovaLive.Api.Hubs;
using NovaLive.Api.Services;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.UseCases.Livestreams.Commands.SendComment;
using NovaLive.Contracts.V1.Livestreams;
using NovaLive.Domain.Common;
using Xunit;

namespace NovaLive.Api.Tests.Livestreams;

public class LivestreamHubTests
{
    private readonly Mock<ISender> _senderMock = new();
    private readonly Mock<ILivestreamCacheManager> _cacheManagerMock = new();
    private readonly Mock<ILivestreamNotificationService> _notificationMock = new();
    private readonly Mock<ILivestreamReactionBatcher> _reactionBatcherMock = new();

    private readonly Mock<HubCallerContext> _contextMock = new();
    private readonly Mock<IGroupManager> _groupsMock = new();
    private readonly IDictionary<object, object?> _items = new Dictionary<object, object?>();

    private readonly Guid _sessionId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private const string ConnectionId = "conn_live_001";

    public LivestreamHubTests()
    {
        _contextMock.Setup(c => c.ConnectionId).Returns(ConnectionId);
        _contextMock.Setup(c => c.Items).Returns(_items);
        _contextMock.Setup(c => c.UserIdentifier).Returns(_userId.ToString());

        var claims = new[]
        {
            new Claim("sub", _userId.ToString()),
            new Claim("name", "Nguyễn Văn Test")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _contextMock.Setup(c => c.User).Returns(new ClaimsPrincipal(identity));
    }

    private LivestreamHub CreateHub()
    {
        var hub = new LivestreamHub(
            _senderMock.Object,
            _cacheManagerMock.Object,
            _notificationMock.Object,
            _reactionBatcherMock.Object)
        {
            Context = _contextMock.Object,
            Groups = _groupsMock.Object
        };

        return hub;
    }

    [Fact]
    public async Task JoinLiveSession_AddsToGroupIncrementsViewerAndBroadcasts()
    {
        _cacheManagerMock.Setup(c => c.IncrementViewerCountAsync(_sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(42);

        var hub = CreateHub();

        var viewerCount = await hub.JoinLiveSession(_sessionId);

        viewerCount.Should().Be(42);
        _items["SessionId"].Should().Be(_sessionId);

        _groupsMock.Verify(g => g.AddToGroupAsync(ConnectionId, $"live_{_sessionId:N}", It.IsAny<CancellationToken>()), Times.Once);
        _cacheManagerMock.Verify(c => c.IncrementViewerCountAsync(_sessionId, It.IsAny<CancellationToken>()), Times.Once);
        _notificationMock.Verify(n => n.BroadcastViewerCountUpdatedAsync(_sessionId, 42, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task JoinLiveSession_WhenAlreadyInSameSession_DoesNotIncrementAgain()
    {
        _items["SessionId"] = _sessionId;
        _cacheManagerMock.Setup(c => c.GetViewerCountAsync(_sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(42);

        var hub = CreateHub();

        var viewerCount = await hub.JoinLiveSession(_sessionId);

        viewerCount.Should().Be(42);
        _cacheManagerMock.Verify(c => c.IncrementViewerCountAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task JoinLiveSession_WhenInDifferentSession_LeavesOldSessionAndJoinsNewOne()
    {
        var oldSessionId = Guid.NewGuid();
        _items["SessionId"] = oldSessionId;

        _cacheManagerMock.Setup(c => c.DecrementViewerCountAsync(oldSessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);
        _cacheManagerMock.Setup(c => c.IncrementViewerCountAsync(_sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(25);

        var hub = CreateHub();

        var viewerCount = await hub.JoinLiveSession(_sessionId);

        viewerCount.Should().Be(25);
        _items["SessionId"].Should().Be(_sessionId);

        _groupsMock.Verify(g => g.RemoveFromGroupAsync(ConnectionId, $"live_{oldSessionId:N}", It.IsAny<CancellationToken>()), Times.Once);
        _cacheManagerMock.Verify(c => c.DecrementViewerCountAsync(oldSessionId, It.IsAny<CancellationToken>()), Times.Once);
        _groupsMock.Verify(g => g.AddToGroupAsync(ConnectionId, $"live_{_sessionId:N}", It.IsAny<CancellationToken>()), Times.Once);
        _cacheManagerMock.Verify(c => c.IncrementViewerCountAsync(_sessionId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LeaveLiveSession_WhenNotInTrackedSession_DoesNotDecrement()
    {
        _cacheManagerMock.Setup(c => c.GetViewerCountAsync(_sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(40);

        var hub = CreateHub();

        var viewerCount = await hub.LeaveLiveSession(_sessionId);

        viewerCount.Should().Be(40);
        _cacheManagerMock.Verify(c => c.DecrementViewerCountAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LeaveLiveSession_RemovesFromGroupDecrementsViewerAndBroadcasts()
    {
        _items["SessionId"] = _sessionId;
        _cacheManagerMock.Setup(c => c.DecrementViewerCountAsync(_sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(41);

        var hub = CreateHub();

        var viewerCount = await hub.LeaveLiveSession(_sessionId);

        viewerCount.Should().Be(41);
        _items.ContainsKey("SessionId").Should().BeFalse();

        _groupsMock.Verify(g => g.RemoveFromGroupAsync(ConnectionId, $"live_{_sessionId:N}", It.IsAny<CancellationToken>()), Times.Once);
        _cacheManagerMock.Verify(c => c.DecrementViewerCountAsync(_sessionId, It.IsAny<CancellationToken>()), Times.Once);
        _notificationMock.Verify(n => n.BroadcastViewerCountUpdatedAsync(_sessionId, 41, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task OnDisconnectedAsync_WhenSessionTracked_DecrementsViewerCount()
    {
        _items["SessionId"] = _sessionId;
        _cacheManagerMock.Setup(c => c.DecrementViewerCountAsync(_sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(40);

        var hub = CreateHub();

        await hub.OnDisconnectedAsync(null);

        _cacheManagerMock.Verify(c => c.DecrementViewerCountAsync(_sessionId, It.IsAny<CancellationToken>()), Times.Once);
        _notificationMock.Verify(n => n.BroadcastViewerCountUpdatedAsync(_sessionId, 40, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendMessageToSession_WithSessionId_DispatchesCommandSuccessfully()
    {
        var expectedResponse = new LivestreamCommentResponse(
            Guid.NewGuid(),
            _sessionId,
            _userId,
            "Nguyễn Văn Test",
            "Shop có freeship không?",
            false,
            DateTimeOffset.UtcNow);

        _senderMock.Setup(s => s.Send(It.Is<SendLivestreamCommentCommand>(c =>
            c.SessionId == _sessionId &&
            c.UserId == _userId &&
            c.DisplayName == "Nguyễn Văn Test" &&
            c.Content == "Shop có freeship không?"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var hub = CreateHub();

        var response = await hub.SendMessageToSession(_sessionId, "Shop có freeship không?");

        response.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task SendMessage_WithImplicitSession_UsesSessionFromContext()
    {
        _items["SessionId"] = _sessionId;

        var expectedResponse = new LivestreamCommentResponse(
            Guid.NewGuid(),
            _sessionId,
            _userId,
            "Nguyễn Văn Test",
            "Màu đen còn không?",
            false,
            DateTimeOffset.UtcNow);

        _senderMock.Setup(s => s.Send(It.Is<SendLivestreamCommentCommand>(c =>
            c.SessionId == _sessionId &&
            c.Content == "Màu đen còn không?"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var hub = CreateHub();

        var response = await hub.SendMessage("Màu đen còn không?");

        response.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task SendMessageToSession_WhenCommandFails_ThrowsHubException()
    {
        _senderMock.Setup(s => s.Send(It.IsAny<SendLivestreamCommentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Validation("Livestream.Invalid", "Nội dung không hợp lệ."));

        var hub = CreateHub();

        var act = async () => await hub.SendMessageToSession(_sessionId, "Hello");

        await act.Should().ThrowAsync<HubException>()
            .WithMessage("Nội dung không hợp lệ.");
    }

    [Fact]
    public void SendReactionToSession_WithSessionId_EnqueuesInReactionBatcher()
    {
        var hub = CreateHub();

        hub.SendReactionToSession(_sessionId, "heart");

        _reactionBatcherMock.Verify(r => r.AddReaction(_sessionId, "heart"), Times.Once);
    }

    [Fact]
    public void SendReaction_WithImplicitSession_UsesContextSessionId()
    {
        _items["SessionId"] = _sessionId;

        var hub = CreateHub();

        hub.SendReaction("fire");

        _reactionBatcherMock.Verify(r => r.AddReaction(_sessionId, "fire"), Times.Once);
    }
}
