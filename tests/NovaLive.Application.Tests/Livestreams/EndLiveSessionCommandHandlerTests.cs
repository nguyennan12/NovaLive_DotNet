using FluentAssertions;
using Moq;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.UseCases.Livestreams;
using NovaLive.Application.UseCases.Livestreams.Commands.EndLiveSession;
using NovaLive.Domain.Common;
using NovaLive.Domain.Livestreams;
using NovaLive.Domain.Products;
using Xunit;

namespace NovaLive.Application.Tests.Livestreams;

public class EndLiveSessionCommandHandlerTests
{
    private readonly Mock<ILivestreamSessionRepository> _sessionRepoMock = new();
    private readonly Mock<ILivestreamProductRepository> _productRepoMock = new();
    private readonly Mock<ISkuRepository> _skuRepoMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly Mock<ICurrentShop> _currentShopMock = new();
    private readonly Mock<IDateTimeProvider> _clockMock = new();
    private readonly Mock<ILivestreamNotificationService> _notificationMock = new();
    private readonly Mock<IAfterCommitActions> _afterCommitMock = new();

    private readonly Guid _shopId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly DateTimeOffset _now = DateTimeOffset.UtcNow;

    public EndLiveSessionCommandHandlerTests()
    {
        _currentUserMock.Setup(u => u.UserId).Returns(_userId);
        _currentUserMock.Setup(u => u.ShopId).Returns(_shopId);
        _currentShopMock.Setup(s => s.ShopId).Returns(_shopId);
        _clockMock.Setup(c => c.UtcNow).Returns(_now);
    }

    [Fact]
    public async Task Handle_WhenValidLiveSession_EndsSessionAndReturnsSummary()
    {
        var startedAt = _now.AddHours(-1);
        var session = new LivestreamSession(_shopId, _userId, "Live Sale", null, "live_abc", LivestreamStatus.Live, startedAt: startedAt);
        var skuId = Guid.NewGuid();
        var sku = new Sku(Guid.NewGuid(), _shopId, "SKU1", null, 100000m, 90000m);
        var product = new LivestreamProduct(session.Id, sku.Id, 70000m, 10, 0, isPinned: true, pinnedAt: startedAt);
        product.RecordSold(5);

        _sessionRepoMock.Setup(r => r.GetByIdAsync(session.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        _productRepoMock.Setup(r => r.GetBySessionIdAsync(session.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([product]);
        _skuRepoMock.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([sku]);

        var handler = new EndLiveSessionCommandHandler(
            _sessionRepoMock.Object,
            _productRepoMock.Object,
            _skuRepoMock.Object,
            _currentUserMock.Object,
            _currentShopMock.Object,
            _clockMock.Object,
            _notificationMock.Object,
            _afterCommitMock.Object);

        var command = new EndLiveSessionCommand(session.Id);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.SessionId.Should().Be(session.Id);
        result.Value.TotalProductsSold.Should().Be(5);
        result.Value.TotalEstimatedRevenue.Should().Be(350000m);
        result.Value.Duration.Should().Be(TimeSpan.FromHours(1));

        session.Status.Should().Be(LivestreamStatus.Ended);
        product.IsPinned.Should().BeFalse();

        _sessionRepoMock.Verify(r => r.Update(session), Times.Once);
        _productRepoMock.Verify(r => r.UpdateRange(It.Is<List<LivestreamProduct>>(l => l.Contains(product))), Times.Once);
        _afterCommitMock.Verify(a => a.Add(It.IsAny<Func<CancellationToken, Task>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenSessionAlreadyEnded_ReturnsSummaryIdempotently()
    {
        var startedAt = _now.AddHours(-2);
        var endedAt = _now.AddHours(-1);
        var session = new LivestreamSession(_shopId, _userId, "Live Sale", null, "live_abc", LivestreamStatus.Live, startedAt: startedAt);
        session.End(endedAt);

        _sessionRepoMock.Setup(r => r.GetByIdAsync(session.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        _productRepoMock.Setup(r => r.GetBySessionIdAsync(session.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new EndLiveSessionCommandHandler(
            _sessionRepoMock.Object,
            _productRepoMock.Object,
            _skuRepoMock.Object,
            _currentUserMock.Object,
            _currentShopMock.Object,
            _clockMock.Object,
            _notificationMock.Object,
            _afterCommitMock.Object);

        var command = new EndLiveSessionCommand(session.Id);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.SessionId.Should().Be(session.Id);
        _sessionRepoMock.Verify(r => r.Update(It.IsAny<LivestreamSession>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenSessionNotLive_ReturnsNotLiveError()
    {
        var session = new LivestreamSession(_shopId, _userId, "Live Sale", null, "live_abc", LivestreamStatus.Scheduled);

        _sessionRepoMock.Setup(r => r.GetByIdAsync(session.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var handler = new EndLiveSessionCommandHandler(
            _sessionRepoMock.Object,
            _productRepoMock.Object,
            _skuRepoMock.Object,
            _currentUserMock.Object,
            _currentShopMock.Object,
            _clockMock.Object,
            _notificationMock.Object,
            _afterCommitMock.Object);

        var command = new EndLiveSessionCommand(session.Id);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(LivestreamErrors.NotLive.Code);
    }

    [Fact]
    public async Task Handle_WhenForbiddenShop_ReturnsForbiddenShopError()
    {
        var otherShopId = Guid.NewGuid();
        var session = new LivestreamSession(otherShopId, _userId, "Live Sale", null, "live_abc", LivestreamStatus.Live);

        _sessionRepoMock.Setup(r => r.GetByIdAsync(session.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var handler = new EndLiveSessionCommandHandler(
            _sessionRepoMock.Object,
            _productRepoMock.Object,
            _skuRepoMock.Object,
            _currentUserMock.Object,
            _currentShopMock.Object,
            _clockMock.Object,
            _notificationMock.Object,
            _afterCommitMock.Object);

        var command = new EndLiveSessionCommand(session.Id);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(LivestreamErrors.ForbiddenShop.Code);
    }
}
