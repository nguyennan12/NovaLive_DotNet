using FluentAssertions;
using Moq;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.UseCases.FlashSales;
using NovaLive.Application.UseCases.FlashSales.Commands.RejectFlashSaleItem;
using NovaLive.Contracts.V1.FlashSales;
using NovaLive.Domain.FlashSales;
using NovaLive.Domain.Products;
using Xunit;

namespace NovaLive.Application.Tests.FlashSales;

public class RejectFlashSaleItemTests
{
    private readonly Mock<IFlashSaleRepository> _flashSaleRepoMock = new();
    private readonly Mock<ISkuRepository> _skuRepoMock = new();
    private readonly Mock<ISpuRepository> _spuRepoMock = new();
    private readonly RejectFlashSaleItemCommandHandler _handler;

    private readonly Guid _itemId = Guid.NewGuid();
    private readonly Guid _campaignId = Guid.NewGuid();
    private readonly Guid _skuId = Guid.NewGuid();
    private readonly Guid _shopId = Guid.NewGuid();

    public RejectFlashSaleItemTests()
    {
        _handler = new RejectFlashSaleItemCommandHandler(
            _flashSaleRepoMock.Object,
            _skuRepoMock.Object,
            _spuRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ItemNotFound_ShouldReturnItemNotFound()
    {
        _flashSaleRepoMock.Setup(r => r.GetItemByIdAsync(_itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FlashSaleItem?)null);

        var req = new RejectFlashSaleItemRequest("Lý do từ chối");
        var result = await _handler.Handle(new RejectFlashSaleItemCommand(_itemId, req), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(FlashSalesErrors.ItemNotFound);
    }

    [Fact]
    public async Task Handle_ItemNotPending_ShouldReturnItemNotPending()
    {
        var item = new FlashSaleItem(_campaignId, _skuId, _shopId, 80_000m, 10, 1);
        item.Approve(); // Already approved

        _flashSaleRepoMock.Setup(r => r.GetItemByIdAsync(_itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        var req = new RejectFlashSaleItemRequest("Lý do");
        var result = await _handler.Handle(new RejectFlashSaleItemCommand(_itemId, req), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(FlashSalesErrors.ItemNotPending);
    }

    [Fact]
    public async Task Handle_PendingItem_ShouldRejectWithReason()
    {
        var item = new FlashSaleItem(_campaignId, _skuId, _shopId, 80_000m, 10, 1);
        _flashSaleRepoMock.Setup(r => r.GetItemByIdAsync(_itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        var sku = new Sku(Guid.NewGuid(), _shopId, "SKU-CODE", null, 100_000m, 95_000m);
        _skuRepoMock.Setup(r => r.GetByIdAsync(_skuId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sku);

        var req = new RejectFlashSaleItemRequest("Giá bán không hợp lý");
        var result = await _handler.Handle(new RejectFlashSaleItemCommand(_itemId, req), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be("Rejected");
        result.Value.RejectionReason.Should().Be("Giá bán không hợp lý");

        _flashSaleRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
