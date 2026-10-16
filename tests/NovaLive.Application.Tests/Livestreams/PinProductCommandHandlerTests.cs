using FluentAssertions;
using Moq;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.UseCases.Livestreams;
using NovaLive.Application.UseCases.Livestreams.Commands.PinProduct;
using NovaLive.Contracts.V1.Livestreams;
using NovaLive.Domain.Common;
using NovaLive.Domain.Livestreams;
using NovaLive.Domain.Products;
using Xunit;
using InventoryItem = NovaLive.Domain.Inventory.Inventory;

namespace NovaLive.Application.Tests.Livestreams;

public class PinProductCommandHandlerTests
{
    private readonly Mock<ILivestreamSessionRepository> _sessionRepoMock = new();
    private readonly Mock<ILivestreamProductRepository> _productRepoMock = new();
    private readonly Mock<ISkuRepository> _skuRepoMock = new();
    private readonly Mock<ISpuRepository> _spuRepoMock = new();
    private readonly Mock<IInventoryRepository> _inventoryRepoMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly Mock<ICurrentShop> _currentShopMock = new();
    private readonly Mock<IDateTimeProvider> _clockMock = new();
    private readonly Mock<ILivestreamNotificationService> _notificationMock = new();
    private readonly Mock<IAfterCommitActions> _afterCommitMock = new();

    private readonly Guid _shopId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly DateTimeOffset _now = DateTimeOffset.UtcNow;

    public PinProductCommandHandlerTests()
    {
        _currentUserMock.Setup(u => u.UserId).Returns(_userId);
        _currentUserMock.Setup(u => u.ShopId).Returns(_shopId);
        _currentShopMock.Setup(s => s.ShopId).Returns(_shopId);
        _clockMock.Setup(c => c.UtcNow).Returns(_now);
    }

    [Fact]
    public async Task Handle_WhenValidRequest_PinsProductAndQueuesBroadcast()
    {
        var session = new LivestreamSession(_shopId, _userId, "Live 1", null, "live_123", LivestreamStatus.Live);
        var spu = new Spu(_shopId, Guid.NewGuid(), "Ao thun", null, "BrandA", "thumb.jpg", null);
        var sku = new Sku(spu.Id, _shopId, "SKU01", null, 100000m, 90000m);
        var inventory = new InventoryItem(sku.Id, _shopId, 50);

        _sessionRepoMock.Setup(r => r.GetByIdAsync(session.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        _skuRepoMock.Setup(r => r.GetByIdAndShopAsync(sku.Id, _shopId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sku);
        _inventoryRepoMock.Setup(r => r.GetBySkuIdAndShopAsync(sku.Id, _shopId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventory);
        _spuRepoMock.Setup(r => r.GetByIdAsync(spu.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(spu);
        _productRepoMock.Setup(r => r.GetBySessionIdAsync(session.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new PinProductCommandHandler(
            _sessionRepoMock.Object,
            _productRepoMock.Object,
            _skuRepoMock.Object,
            _spuRepoMock.Object,
            _inventoryRepoMock.Object,
            _currentUserMock.Object,
            _currentShopMock.Object,
            _clockMock.Object,
            _notificationMock.Object,
            _afterCommitMock.Object);

        var request = new PinProductRequest(sku.Id, 80000m, 10, 180);
        var command = new PinProductCommand(session.Id, request);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.SkuId.Should().Be(sku.Id);
        result.Value.FlashPrice.Should().Be(80000m);
        result.Value.OriginalPrice.Should().Be(100000m);
        result.Value.SellPrice.Should().Be(90000m);
        result.Value.EffectivePrice.Should().Be(80000m);
        result.Value.ExpiresAt.Should().Be(_now.AddSeconds(180));

        _productRepoMock.Verify(r => r.AddAsync(It.Is<LivestreamProduct>(p =>
            p.SessionId == session.Id &&
            p.SkuId == sku.Id &&
            p.IsPinned == true &&
            p.FlashPrice == 80000m &&
            p.QuantityLimit == 10), It.IsAny<CancellationToken>()), Times.Once);

        _afterCommitMock.Verify(a => a.Add(It.IsAny<Func<CancellationToken, Task>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenFlashPriceExceedsSellPrice_ReturnsInvalidFlashPrice()
    {
        var session = new LivestreamSession(_shopId, _userId, "Live 1", null, "live_123", LivestreamStatus.Live);
        var sku = new Sku(Guid.NewGuid(), _shopId, "SKU01", null, 100000m, 90000m);

        _sessionRepoMock.Setup(r => r.GetByIdAsync(session.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        _skuRepoMock.Setup(r => r.GetByIdAndShopAsync(sku.Id, _shopId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sku);

        var handler = new PinProductCommandHandler(
            _sessionRepoMock.Object,
            _productRepoMock.Object,
            _skuRepoMock.Object,
            _spuRepoMock.Object,
            _inventoryRepoMock.Object,
            _currentUserMock.Object,
            _currentShopMock.Object,
            _clockMock.Object,
            _notificationMock.Object,
            _afterCommitMock.Object);

        var request = new PinProductRequest(sku.Id, 95000m, 10, 120);
        var command = new PinProductCommand(session.Id, request);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(LivestreamErrors.InvalidFlashPrice.Code);
    }

    [Fact]
    public async Task Handle_WhenQuantityLimitExceedsStock_ReturnsQuantityLimitExceedsStock()
    {
        var session = new LivestreamSession(_shopId, _userId, "Live 1", null, "live_123", LivestreamStatus.Live);
        var sku = new Sku(Guid.NewGuid(), _shopId, "SKU01", null, 100000m, 90000m);
        var inventory = new InventoryItem(sku.Id, _shopId, 5);

        _sessionRepoMock.Setup(r => r.GetByIdAsync(session.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        _skuRepoMock.Setup(r => r.GetByIdAndShopAsync(sku.Id, _shopId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sku);
        _inventoryRepoMock.Setup(r => r.GetBySkuIdAndShopAsync(sku.Id, _shopId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventory);

        var handler = new PinProductCommandHandler(
            _sessionRepoMock.Object,
            _productRepoMock.Object,
            _skuRepoMock.Object,
            _spuRepoMock.Object,
            _inventoryRepoMock.Object,
            _currentUserMock.Object,
            _currentShopMock.Object,
            _clockMock.Object,
            _notificationMock.Object,
            _afterCommitMock.Object);

        var request = new PinProductRequest(sku.Id, 80000m, 20, 120);
        var command = new PinProductCommand(session.Id, request);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(LivestreamErrors.QuantityLimitExceedsStock.Code);
    }

    [Fact]
    public async Task Handle_WhenSessionNotLive_ReturnsNotLive()
    {
        var session = new LivestreamSession(_shopId, _userId, "Live 1", null, "live_123", LivestreamStatus.Ended);

        _sessionRepoMock.Setup(r => r.GetByIdAsync(session.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var handler = new PinProductCommandHandler(
            _sessionRepoMock.Object,
            _productRepoMock.Object,
            _skuRepoMock.Object,
            _spuRepoMock.Object,
            _inventoryRepoMock.Object,
            _currentUserMock.Object,
            _currentShopMock.Object,
            _clockMock.Object,
            _notificationMock.Object,
            _afterCommitMock.Object);

        var request = new PinProductRequest(Guid.NewGuid(), 80000m, 5, 120);
        var command = new PinProductCommand(session.Id, request);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(LivestreamErrors.NotLive.Code);
    }

    [Fact]
    public async Task Handle_WhenAnotherProductAlreadyPinned_UnpinsIt()
    {
        var session = new LivestreamSession(_shopId, _userId, "Live 1", null, "live_123", LivestreamStatus.Live);
        var spu = new Spu(_shopId, Guid.NewGuid(), "Ao thun", null, "BrandA", "thumb.jpg", null);
        var oldSkuId = Guid.NewGuid();
        var newSku = new Sku(spu.Id, _shopId, "SKU02", null, 100000m, 90000m);
        var inventory = new InventoryItem(newSku.Id, _shopId, 50);

        var oldProduct = new LivestreamProduct(session.Id, oldSkuId, 50000m, 10, 0, isPinned: true, pinnedAt: _now.AddMinutes(-5));

        _sessionRepoMock.Setup(r => r.GetByIdAsync(session.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        _skuRepoMock.Setup(r => r.GetByIdAndShopAsync(newSku.Id, _shopId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newSku);
        _inventoryRepoMock.Setup(r => r.GetBySkuIdAndShopAsync(newSku.Id, _shopId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventory);
        _spuRepoMock.Setup(r => r.GetByIdAsync(spu.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(spu);
        _productRepoMock.Setup(r => r.GetBySessionIdAsync(session.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([oldProduct]);

        var handler = new PinProductCommandHandler(
            _sessionRepoMock.Object,
            _productRepoMock.Object,
            _skuRepoMock.Object,
            _spuRepoMock.Object,
            _inventoryRepoMock.Object,
            _currentUserMock.Object,
            _currentShopMock.Object,
            _clockMock.Object,
            _notificationMock.Object,
            _afterCommitMock.Object);

        var request = new PinProductRequest(newSku.Id, 80000m, 10, 120);
        var command = new PinProductCommand(session.Id, request);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        oldProduct.IsPinned.Should().BeFalse();
        _productRepoMock.Verify(r => r.UpdateRange(It.Is<List<LivestreamProduct>>(list => list.Contains(oldProduct))), Times.Once);
    }
}
