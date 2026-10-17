using FluentAssertions;
using Moq;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.UseCases.Livestreams;
using NovaLive.Application.UseCases.Livestreams.Commands.SendComment;
using NovaLive.Contracts.V1.Livestreams;
using NovaLive.Domain.Common;
using NovaLive.Domain.Livestreams;
using Xunit;

namespace NovaLive.Application.Tests.Livestreams;

public class SendLivestreamCommentCommandHandlerTests
{
    private readonly Mock<ILivestreamSessionRepository> _sessionRepoMock = new();
    private readonly Mock<ILivestreamCommentRepository> _commentRepoMock = new();
    private readonly Mock<ILivestreamCacheManager> _cacheManagerMock = new();
    private readonly Mock<ILivestreamNotificationService> _notificationMock = new();
    private readonly Mock<IDateTimeProvider> _clockMock = new();
    private readonly Mock<IAfterCommitActions> _afterCommitMock = new();

    private readonly Guid _sessionId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly DateTimeOffset _now = DateTimeOffset.UtcNow;

    public SendLivestreamCommentCommandHandlerTests()
    {
        _clockMock.Setup(c => c.UtcNow).Returns(_now);
    }

    [Fact]
    public async Task Handle_WhenValidCommand_AddsCommentToDbAndRegistersCacheAndNotification()
    {
        var session = new LivestreamSession(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Live Bán Hàng",
            null,
            $"live_{_sessionId:N}",
            LivestreamStatus.Live,
            startedAt: _now,
            id: _sessionId);

        _sessionRepoMock.Setup(r => r.GetByIdAsync(_sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        Func<CancellationToken, Task>? registeredAction = null;
        _afterCommitMock.Setup(a => a.Add(It.IsAny<Func<CancellationToken, Task>>()))
            .Callback<Func<CancellationToken, Task>>(action => registeredAction = action);

        var handler = new SendLivestreamCommentCommandHandler(
            _sessionRepoMock.Object,
            _commentRepoMock.Object,
            _cacheManagerMock.Object,
            _notificationMock.Object,
            _clockMock.Object,
            _afterCommitMock.Object);

        var command = new SendLivestreamCommentCommand(
            _sessionId,
            _userId,
            "Trần Văn B",
            "Sản phẩm rất đẹp!");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.SessionId.Should().Be(_sessionId);
        result.Value.UserId.Should().Be(_userId);
        result.Value.DisplayName.Should().Be("Trần Văn B");
        result.Value.Content.Should().Be("Sản phẩm rất đẹp!");
        result.Value.CreatedAt.Should().Be(_now);

        _commentRepoMock.Verify(c => c.AddAsync(It.Is<LivestreamComment>(comment =>
            comment.SessionId == _sessionId &&
            comment.UserId == _userId &&
            comment.DisplayName == "Trần Văn B" &&
            comment.Content == "Sản phẩm rất đẹp!"), It.IsAny<CancellationToken>()), Times.Once);

        _afterCommitMock.Verify(a => a.Add(It.IsAny<Func<CancellationToken, Task>>()), Times.Once);

        registeredAction.Should().NotBeNull();
        await registeredAction!(CancellationToken.None);

        _cacheManagerMock.Verify(c => c.AddCommentAsync(_sessionId, It.Is<LivestreamCommentResponse>(r =>
            r.Content == "Sản phẩm rất đẹp!"), It.IsAny<CancellationToken>()), Times.Once);

        _notificationMock.Verify(n => n.BroadcastReceiveMessageAsync(_sessionId, It.Is<LivestreamCommentResponse>(r =>
            r.Content == "Sản phẩm rất đẹp!"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenSessionNotFound_ReturnsNotFoundError()
    {
        _sessionRepoMock.Setup(r => r.GetByIdAsync(_sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((LivestreamSession?)null);

        var handler = new SendLivestreamCommentCommandHandler(
            _sessionRepoMock.Object,
            _commentRepoMock.Object,
            _cacheManagerMock.Object,
            _notificationMock.Object,
            _clockMock.Object,
            _afterCommitMock.Object);

        var command = new SendLivestreamCommentCommand(_sessionId, _userId, "User", "Hello");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(LivestreamErrors.NotFound.Code);
    }

    [Fact]
    public async Task Handle_WhenSessionNotLive_ReturnsNotLiveError()
    {
        var session = LivestreamSession.CreateLive(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Live Bán Hàng",
            null,
            $"live_{_sessionId:N}",
            _now);
        session.End(_now.AddHours(1));

        _sessionRepoMock.Setup(r => r.GetByIdAsync(_sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var handler = new SendLivestreamCommentCommandHandler(
            _sessionRepoMock.Object,
            _commentRepoMock.Object,
            _cacheManagerMock.Object,
            _notificationMock.Object,
            _clockMock.Object,
            _afterCommitMock.Object);

        var command = new SendLivestreamCommentCommand(_sessionId, _userId, "User", "Hello");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(LivestreamErrors.NotLive.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validator_WhenContentEmpty_FailsValidation(string invalidContent)
    {
        var validator = new SendLivestreamCommentCommandValidator();
        var command = new SendLivestreamCommentCommand(_sessionId, _userId, "User", invalidContent);

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SendLivestreamCommentCommand.Content));
    }

    [Fact]
    public void Validator_WhenContentExceeds500Chars_FailsValidation()
    {
        var validator = new SendLivestreamCommentCommandValidator();
        var longContent = new string('a', 501);
        var command = new SendLivestreamCommentCommand(_sessionId, _userId, "User", longContent);

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SendLivestreamCommentCommand.Content));
    }

    [Fact]
    public void Validator_WhenDisplayNameEmpty_FailsValidation()
    {
        var validator = new SendLivestreamCommentCommandValidator();
        var command = new SendLivestreamCommentCommand(_sessionId, _userId, "", "Valid comment");

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SendLivestreamCommentCommand.DisplayName));
    }
}
