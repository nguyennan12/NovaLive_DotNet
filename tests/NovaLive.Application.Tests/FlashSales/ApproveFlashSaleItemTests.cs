using FluentAssertions;
using Moq;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.UseCases.FlashSales;
using NovaLive.Application.UseCases.FlashSales.Commands.ApproveFlashSaleItem;
using NovaLive.Domain.FlashSales;
using NovaLive.Domain.Products;
using Xunit;

namespace NovaLive.Application.Tests.FlashSales;

public class ApproveFlashSaleItemTests
{
    private readonly Mock<IFlashSaleRepository> _flashSaleRepoMock = new();
    private readonly Mock<ISkuRepository> _skuRepoMock = new();
    private readonly Mock<ISpuRepository> _spuRepoMock = new();
    private readonly Mock<ICacheService> _cacheMock = new();
    private readonly ApproveFlashSaleItemCommandHandler _handler;

    private readonly Guid _itemId = Guid.NewGuid();
    private readonly Guid _campaignId = Guid.NewGuid();
    private readonly Guid _skuId = Guid.NewGuid();
    private readonly Guid _shopId = Guid.NewGuid();

    public ApproveFlashSaleItemTests()
    {
        _handler = new ApproveFlashSaleItemCommandHandler(
            _flashSaleRepoMock.Object,
            _skuRepoMock.Object,
            _spuRepoMock.Object,
            _cacheMock.Object);
    }

    [Fact]
    public async Task Handle_ItemNotFound_ShouldReturnItemNotFound()
    {
        _flashSaleRepoMock.Setup(r => r.GetItemByIdAsync(_itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FlashSaleItem?)null);

        var result = await _handler.Handle(new ApproveFlashSaleItemCommand(_itemId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(FlashSalesErrors.ItemNotFound);
    }

    [Fact]
    public async Task Handle_ItemNotPending_ShouldReturnItemNotPending()
    {
        var item = new FlashSaleItem(_campaignId, _skuId, _shopId, 80_000m, 10, 1);
        item.Approve(); // already approved

        _flashSaleRepoMock.Setup(r => r.GetItemByIdAsync(_itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        var result = await _handler.Handle(new ApproveFlashSaleItemCommand(_itemId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(FlashSalesErrors.ItemNotPending);
    }

    [Fact]
    public async Task Handle_PendingItem_ShouldApproveAndInvalidateCache()
    {
        var item = new FlashSaleItem(_campaignId, _skuId, _shopId, 80_000m, 10, 1);
        _flashSaleRepoMock.Setup(r => r.GetItemByIdAsync(_itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        var sku = new Sku(Guid.NewGuid(), _shopId, "SKU-CODE", null, 100_000m, 95_000m);
        _skuRepoMock.Setup(r => r.GetByIdAsync(_skuId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sku);

        var result = await _handler.Handle(new ApproveFlashSaleItemCommand(_itemId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be("Approved");

        _flashSaleRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(c => c.RemoveAsync("flashsales:active", It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(c => c.RemoveByPrefixAsync($"flashsales:campaign:{item.CampaignId}", It.IsAny<CancellationToken>()), Times.Once);
    }
}
