using FluentAssertions;
using Moq;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.UseCases.FlashSales;
using NovaLive.Application.UseCases.FlashSales.Queries.GetFlashSaleProducts;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.FlashSales;
using NovaLive.Domain.FlashSales;
using NovaLive.Domain.Products;
using Xunit;

namespace NovaLive.Application.Tests.FlashSales;

public class GetFlashSaleProductsTests
{
    private readonly Mock<IFlashSaleRepository> _flashSaleRepoMock = new();
    private readonly Mock<ISkuRepository> _skuRepoMock = new();
    private readonly Mock<ISpuRepository> _spuRepoMock = new();
    private readonly Mock<ICacheService> _cacheMock = new();
    private readonly GetFlashSaleProductsQueryHandler _handler;

    private readonly Guid _campaignId = Guid.NewGuid();
    private readonly Guid _shopId = Guid.NewGuid();
    private readonly Guid _spuId = Guid.NewGuid();
    private readonly Guid _skuId = Guid.NewGuid();

    public GetFlashSaleProductsTests()
    {
        _cacheMock
            .Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<CancellationToken, Task<PagedResult<FlashSaleItemResponse>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<CancellationToken, Task<PagedResult<FlashSaleItemResponse>>>, TimeSpan?, CancellationToken>(
                (key, factory, ttl, ct) => factory(ct));

        _handler = new GetFlashSaleProductsQueryHandler(
            _flashSaleRepoMock.Object,
            _skuRepoMock.Object,
            _spuRepoMock.Object,
            _cacheMock.Object);
    }

    [Fact]
    public async Task Handle_CampaignNotFound_ShouldReturnError()
    {
        _flashSaleRepoMock.Setup(r => r.GetCampaignByIdAsync(_campaignId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FlashSaleCampaign?)null);

        var result = await _handler.Handle(new GetFlashSaleProductsQuery(_campaignId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(FlashSalesErrors.CampaignNotFound);
    }

    [Fact]
    public async Task Handle_ApprovedItems_ShouldEnrichAndReturnPagedResult()
    {
        var campaign = new FlashSaleCampaign("Camp", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(2));
        _flashSaleRepoMock.Setup(r => r.GetCampaignByIdAsync(_campaignId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);

        var spu = new Spu(_shopId, Guid.NewGuid(), "SPU Awesome Product", null, null, "https://image.png", null);
        var sku = new Sku(spu.Id, _shopId, "SKU-CODE", null, 100_000m, 95_000m);
        var item = new FlashSaleItem(_campaignId, sku.Id, _shopId, 80_000m, 10, 1);
        item.Approve();

        _flashSaleRepoMock.Setup(r => r.GetApprovedItemsByCampaignAsync(_campaignId, 0, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(([item], 1));

        _skuRepoMock.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([sku]);

        _spuRepoMock.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([spu]);

        var result = await _handler.Handle(new GetFlashSaleProductsQuery(_campaignId, 1, 20), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Total.Should().Be(1);
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items[0].SpuName.Should().Be("SPU Awesome Product");
        result.Value.Items[0].ThumbnailUrl.Should().Be("https://image.png");
        result.Value.Items[0].FlashPrice.Should().Be(80_000m);
    }
}
