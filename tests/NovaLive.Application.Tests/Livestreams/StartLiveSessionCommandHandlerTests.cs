using FluentAssertions;
using Moq;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Abstractions.ThirdParty;
using NovaLive.Application.UseCases.Livestreams;
using NovaLive.Application.UseCases.Livestreams.Commands.StartLiveSession;
using NovaLive.Contracts.V1.Livestreams;
using NovaLive.Domain.Common;
using NovaLive.Domain.Livestreams;
using NovaLive.Domain.Shops;
using Xunit;

namespace NovaLive.Application.Tests.Livestreams;

public class StartLiveSessionCommandHandlerTests
{
    private readonly Mock<ILivestreamSessionRepository> _sessionRepoMock = new();
    private readonly Mock<IShopRepository> _shopRepoMock = new();
    private readonly Mock<IShopAddressRepository> _shopAddressRepoMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly Mock<ICurrentShop> _currentShopMock = new();
    private readonly Mock<IAgoraTokenService> _agoraServiceMock = new();
    private readonly Mock<IDateTimeProvider> _clockMock = new();

    private readonly Guid _shopId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly DateTimeOffset _now = DateTimeOffset.UtcNow;

    public StartLiveSessionCommandHandlerTests()
    {
        _currentUserMock.Setup(u => u.UserId).Returns(_userId);
        _currentUserMock.Setup(u => u.ShopId).Returns(_shopId);
        _currentShopMock.Setup(s => s.ShopId).Returns(_shopId);
        _clockMock.Setup(c => c.UtcNow).Returns(_now);

        var shop = new Shop(_userId, "Test Shop", "test-shop");
        _shopRepoMock.Setup(s => s.GetActiveByIdAsync(_shopId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shop);

        _agoraServiceMock.Setup(a => a.GeneratePublisherToken(It.IsAny<string>(), _userId.ToString(), null))
            .Returns("mock_publisher_token_007");
    }

    [Fact]
    public async Task Handle_WhenValidRequest_CreatesSessionAndReturnsPublisherToken()
    {
        _sessionRepoMock.Setup(s => s.HasActiveLiveSessionAsync(_shopId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new StartLiveSessionCommandHandler(
            _sessionRepoMock.Object,
            _shopRepoMock.Object,
            _shopAddressRepoMock.Object,
            _currentUserMock.Object,
            _currentShopMock.Object,
            _agoraServiceMock.Object,
            _clockMock.Object);

        var command = new StartLiveSessionCommand(new StartLivestreamRequest("Săn Sale Siêu To", null, null));

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.PublisherToken.Should().Be("mock_publisher_token_007");
        result.Value.ShopName.Should().Be("Test Shop");
        result.Value.ChannelName.Should().StartWith("live_");

        _sessionRepoMock.Verify(s => s.AddAsync(It.Is<LivestreamSession>(sess =>
            sess.ShopId == _shopId &&
            sess.HostId == _userId &&
            sess.Title == "Săn Sale Siêu To" &&
            sess.Status == LivestreamStatus.Live), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenShopAlreadyHasActiveLive_ReturnsConflictError()
    {
        _sessionRepoMock.Setup(s => s.HasActiveLiveSessionAsync(_shopId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new StartLiveSessionCommandHandler(
            _sessionRepoMock.Object,
            _shopRepoMock.Object,
            _shopAddressRepoMock.Object,
            _currentUserMock.Object,
            _currentShopMock.Object,
            _agoraServiceMock.Object,
            _clockMock.Object);

        var command = new StartLiveSessionCommand(new StartLivestreamRequest("Săn Sale", null, null));

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(LivestreamErrors.ShopAlreadyHasActiveLive.Code);
    }

    [Fact]
    public async Task Handle_WhenWarehouseAddressNotFound_ReturnsNotFoundError()
    {
        _sessionRepoMock.Setup(s => s.HasActiveLiveSessionAsync(_shopId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var invalidWarehouseId = Guid.NewGuid();
        _shopAddressRepoMock.Setup(a => a.FindForShopAsync(invalidWarehouseId, _shopId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ShopAddress?)null);

        var handler = new StartLiveSessionCommandHandler(
            _sessionRepoMock.Object,
            _shopRepoMock.Object,
            _shopAddressRepoMock.Object,
            _currentUserMock.Object,
            _currentShopMock.Object,
            _agoraServiceMock.Object,
            _clockMock.Object);

        var command = new StartLiveSessionCommand(new StartLivestreamRequest("Săn Sale", null, invalidWarehouseId));

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(LivestreamErrors.WarehouseAddressNotFound.Code);
    }
}
