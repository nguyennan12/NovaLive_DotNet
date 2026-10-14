using FluentAssertions;
using Moq;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.UseCases.FlashSales;
using NovaLive.Application.UseCases.FlashSales.Commands.RegisterSkuForFlashSale;
using NovaLive.Contracts.V1.FlashSales;
using NovaLive.Domain.Common;
using NovaLive.Domain.FlashSales;
using NovaLive.Domain.Products;
using Xunit;
using InventoryEntity = NovaLive.Domain.Inventory.Inventory;

namespace NovaLive.Application.Tests.FlashSales;

public class RegisterSkuForFlashSaleTests
{
    private readonly Mock<IFlashSaleRepository> _flashSaleRepoMock = new();
    private readonly Mock<ISkuRepository> _skuRepoMock = new();
    private readonly Mock<ISpuRepository> _spuRepoMock = new();
    private readonly Mock<IInventoryRepository> _inventoryRepoMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly Mock<IDateTimeProvider> _clockMock = new();
    private readonly RegisterSkuForFlashSaleCommandHandler _handler;

    private readonly Guid _shopId = Guid.NewGuid();
    private readonly Guid _campaignId = Guid.NewGuid();
    private readonly Guid _skuId = Guid.NewGuid();
    private readonly Guid _spuId = Guid.NewGuid();
    private readonly DateTimeOffset _now = DateTimeOffset.UtcNow;

    public RegisterSkuForFlashSaleTests()
    {
        _currentUserMock.Setup(u => u.ShopId).Returns(_shopId);
        _clockMock.Setup(c => c.UtcNow).Returns(_now);

        _handler = new RegisterSkuForFlashSaleCommandHandler(
            _flashSaleRepoMock.Object,
            _skuRepoMock.Object,
            _spuRepoMock.Object,
            _inventoryRepoMock.Object,
            _currentUserMock.Object,
            _clockMock.Object);
    }

    [Fact]
    public async Task Handle_UnauthorizedShop_ShouldReturnUnauthorized()
    {
        _currentUserMock.Setup(u => u.ShopId).Returns((Guid?)null);

        var req = new RegisterFlashSaleSkuRequest(_skuId, 80_000m, 10, 1);
        var result = await _handler.Handle(new RegisterSkuForFlashSaleCommand(_campaignId, req), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(FlashSalesErrors.UnauthorizedShop);
    }

    [Fact]
    public async Task Handle_CampaignNotFound_ShouldReturnCampaignNotFound()
    {
        _flashSaleRepoMock.Setup(r => r.GetCampaignByIdAsync(_campaignId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FlashSaleCampaign?)null);

        var req = new RegisterFlashSaleSkuRequest(_skuId, 80_000m, 10, 1);
        var result = await _handler.Handle(new RegisterSkuForFlashSaleCommand(_campaignId, req), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(FlashSalesErrors.CampaignNotFound);
    }

    [Fact]
    public async Task Handle_CampaignAlreadyStarted_ShouldReturnRegistrationClosed()
    {
        // Campaign started 10 minutes ago
        var campaign = new FlashSaleCampaign("Started Campaign", _now.AddMinutes(-10), _now.AddHours(1));
        _flashSaleRepoMock.Setup(r => r.GetCampaignByIdAsync(_campaignId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);

        var req = new RegisterFlashSaleSkuRequest(_skuId, 80_000m, 10, 1);
        var result = await _handler.Handle(new RegisterSkuForFlashSaleCommand(_campaignId, req), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(FlashSalesErrors.CampaignRegistrationClosed);
    }

    [Fact]
    public async Task Handle_SkuNotBelongToShop_ShouldReturnForbidden()
    {
        var campaign = new FlashSaleCampaign("Upcoming", _now.AddHours(1), _now.AddHours(2));
        _flashSaleRepoMock.Setup(r => r.GetCampaignByIdAsync(_campaignId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);

        var otherShopId = Guid.NewGuid();
        var sku = new Sku(_spuId, otherShopId, "SKU-OTHER", null, 100_000m, 100_000m);
        _skuRepoMock.Setup(r => r.GetByIdAsync(_skuId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sku);

        var req = new RegisterFlashSaleSkuRequest(_skuId, 80_000m, 10, 1);
        var result = await _handler.Handle(new RegisterSkuForFlashSaleCommand(_campaignId, req), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(FlashSalesErrors.SkuNotBelongToShop);
    }

    [Fact]
    public async Task Handle_PriceNot10PercentLowerThanSellPrice_ShouldReturnError()
    {
        var campaign = new FlashSaleCampaign("Upcoming", _now.AddHours(1), _now.AddHours(2));
        _flashSaleRepoMock.Setup(r => r.GetCampaignByIdAsync(_campaignId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);

        // SellPrice = 100,000 => Flash price must be <= 90,000. Here it's 95,000 (only 5% off)
        var sku = new Sku(_spuId, _shopId, "SKU-1", null, 120_000m, 100_000m);
        _skuRepoMock.Setup(r => r.GetByIdAsync(_skuId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sku);

        var req = new RegisterFlashSaleSkuRequest(_skuId, 95_000m, 10, 1);
        var result = await _handler.Handle(new RegisterSkuForFlashSaleCommand(_campaignId, req), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(FlashSalesErrors.FlashPriceMustBe10PercentLower);
    }

    [Fact]
    public async Task Handle_InsufficientInventory_ShouldReturnInsufficientInventoryStock()
    {
        var campaign = new FlashSaleCampaign("Upcoming", _now.AddHours(1), _now.AddHours(2));
        _flashSaleRepoMock.Setup(r => r.GetCampaignByIdAsync(_campaignId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);

        var sku = new Sku(_spuId, _shopId, "SKU-1", null, 100_000m, 100_000m);
        _skuRepoMock.Setup(r => r.GetByIdAsync(_skuId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sku);

        // Inventory has 5 available, seller registers 10
        var inventory = new InventoryEntity(sku.Id, _shopId, 5, 1);
        _inventoryRepoMock.Setup(r => r.GetBySkuIdAndShopAsync(sku.Id, _shopId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventory);

        var req = new RegisterFlashSaleSkuRequest(_skuId, 80_000m, 10, 1);
        var result = await _handler.Handle(new RegisterSkuForFlashSaleCommand(_campaignId, req), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(FlashSalesErrors.InsufficientInventoryStock);
    }

    [Fact]
    public async Task Handle_OverlappingCampaign_ShouldReturnError()
    {
        var campaign = new FlashSaleCampaign("Upcoming", _now.AddHours(1), _now.AddHours(2));
        _flashSaleRepoMock.Setup(r => r.GetCampaignByIdAsync(_campaignId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);

        var sku = new Sku(_spuId, _shopId, "SKU-1", null, 100_000m, 100_000m);
        _skuRepoMock.Setup(r => r.GetByIdAsync(_skuId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sku);

        var inventory = new InventoryEntity(sku.Id, _shopId, 50, 1);
        _inventoryRepoMock.Setup(r => r.GetBySkuIdAndShopAsync(sku.Id, _shopId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventory);

        _flashSaleRepoMock.Setup(r => r.HasOverlappingFlashSaleAsync(sku.Id, campaign.StartAt, campaign.EndAt, campaign.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var req = new RegisterFlashSaleSkuRequest(_skuId, 80_000m, 10, 1);
        var result = await _handler.Handle(new RegisterSkuForFlashSaleCommand(_campaignId, req), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(FlashSalesErrors.SkuOverlapsWithAnotherCampaign);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldCreatePendingItemAndReturnResponse()
    {
        var campaign = new FlashSaleCampaign("Upcoming", _now.AddHours(1), _now.AddHours(2));
        _flashSaleRepoMock.Setup(r => r.GetCampaignByIdAsync(_campaignId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);

        var sku = new Sku(_spuId, _shopId, "SKU-1", null, 100_000m, 100_000m);
        _skuRepoMock.Setup(r => r.GetByIdAsync(_skuId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sku);

        var inventory = new InventoryEntity(sku.Id, _shopId, 50, 1);
        _inventoryRepoMock.Setup(r => r.GetBySkuIdAndShopAsync(sku.Id, _shopId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventory);

        _flashSaleRepoMock.Setup(r => r.HasOverlappingFlashSaleAsync(sku.Id, campaign.StartAt, campaign.EndAt, campaign.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _flashSaleRepoMock.Setup(r => r.ExistsItemAsync(campaign.Id, sku.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var spu = new Spu(_shopId, Guid.NewGuid(), "SPU Product Name", "Desc", "Brand", "https://thumb.jpg", null);
        _spuRepoMock.Setup(r => r.GetByIdAsync(_spuId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(spu);

        var req = new RegisterFlashSaleSkuRequest(_skuId, 85_000m, 10, 2);
        var result = await _handler.Handle(new RegisterSkuForFlashSaleCommand(_campaignId, req), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.FlashPrice.Should().Be(85_000m);
        result.Value.Quantity.Should().Be(10);
        result.Value.PerUserLimit.Should().Be(2);
        result.Value.Status.Should().Be("Pending");
        result.Value.SpuName.Should().Be("SPU Product Name");
        result.Value.ThumbnailUrl.Should().Be("https://thumb.jpg");

        _flashSaleRepoMock.Verify(r => r.AddItemAsync(It.IsAny<FlashSaleItem>(), It.IsAny<CancellationToken>()), Times.Once);
        _flashSaleRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
