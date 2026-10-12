using FluentAssertions;
using Moq;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.UseCases.Products.Queries.SearchProducts;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Products;
using Xunit;

namespace NovaLive.Application.Tests.Products;

public class SearchProductsQueryHandlerTests
{
    private readonly Mock<IProductQueryService> _queryServiceMock = new();
    private readonly Mock<ICacheService> _cacheServiceMock = new();
    private readonly SearchProductsQueryHandler _handler;

    public SearchProductsQueryHandlerTests()
    {
        _handler = new SearchProductsQueryHandler(_queryServiceMock.Object, _cacheServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCacheHit_ShouldReturnCachedResultWithoutCallingQueryService()
    {
        // Arrange
        var request = new ProductSearchRequest(
            Keyword: "bàn phím cơ",
            CategoryId: Guid.NewGuid(),
            ShopId: Guid.NewGuid(),
            MinPrice: 100000m,
            MaxPrice: 500000m,
            SortBy: "price_asc",
            Page: 1,
            Size: 20);

        var cachedItems = new List<ProductSummaryResponse>
        {
            new(
                Id: Guid.NewGuid(),
                ShopId: request.ShopId!.Value,
                ShopName: "Tech Store",
                CategoryId: request.CategoryId!.Value,
                CategoryName: "Bàn phím",
                Name: "Bàn phím cơ AKKO",
                ThumbnailUrl: "https://example.com/akko.jpg",
                MinPrice: 200000m,
                MaxPrice: 400000m,
                Rating: 5.0,
                SoldCount: 15,
                SearchRank: 0.85m)
        };
        var cachedResult = PagedResult<ProductSummaryResponse>.Create(cachedItems, 1, 20, 1);

        _cacheServiceMock
            .Setup(c => c.GetAsync<PagedResult<ProductSummaryResponse>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedResult);

        var query = new SearchProductsQuery(request);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(cachedResult);
        _queryServiceMock.Verify(s => s.SearchAsync(It.IsAny<ProductSearchRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        _cacheServiceMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<PagedResult<ProductSummaryResponse>>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCacheMiss_ShouldCallQueryServiceAndCacheResultFor60Seconds()
    {
        // Arrange
        var request = new ProductSearchRequest(
            Keyword: "áo thun",
            Page: 1,
            Size: 10);

        var items = new List<ProductSummaryResponse>
        {
            new(
                Id: Guid.NewGuid(),
                ShopId: Guid.NewGuid(),
                ShopName: "Fashion Hub",
                CategoryId: Guid.NewGuid(),
                CategoryName: "Thời trang",
                Name: "Áo thun cotton",
                ThumbnailUrl: null,
                MinPrice: 99000m,
                MaxPrice: 150000m,
                Rating: 4.8,
                SoldCount: 120,
                SearchRank: 0.9m)
        };
        var serviceResult = PagedResult<ProductSummaryResponse>.Create(items, 1, 10, 1);

        _cacheServiceMock
            .Setup(c => c.GetAsync<PagedResult<ProductSummaryResponse>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PagedResult<ProductSummaryResponse>?)null);

        _queryServiceMock
            .Setup(s => s.SearchAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        var query = new SearchProductsQuery(request);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items[0].Name.Should().Be("Áo thun cotton");

        _queryServiceMock.Verify(s => s.SearchAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.SetAsync(
            It.Is<string>(k => k.Contains("áo thun")),
            serviceResult,
            It.Is<TimeSpan?>(t => t == TimeSpan.FromSeconds(60)),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
