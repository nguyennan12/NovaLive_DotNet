using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NovaLive.Api.Controllers.Products;
using NovaLive.Application.UseCases.Products.Queries.GetPublicProductDetail;
using NovaLive.Application.UseCases.Products.Queries.SearchProducts;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Products;
using NovaLive.Domain.Common;
using Xunit;

namespace NovaLive.Api.Tests.Products;

public class ProductsControllerTests
{
    private readonly Mock<ISender> _senderMock = new();
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _controller = new ProductsController(_senderMock.Object);
    }

    [Fact]
    public async Task GetProducts_ShouldSendSearchProductsQuery_AndReturnOk()
    {
        // Arrange
        var request = new ProductSearchRequest(Keyword: "laptop", Page: 1, Size: 10);
        var pagedResult = PagedResult<ProductSummaryResponse>.Create([], 1, 10, 0);

        _senderMock
            .Setup(s => s.Send(It.Is<SearchProductsQuery>(q => q.Request == request), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(pagedResult));

        // Act
        var result = await _controller.GetProducts(request, CancellationToken.None);

        // Assert
        var objResult = result.Should().BeOfType<ObjectResult>().Subject;
        objResult.StatusCode.Should().Be(200);
        var response = objResult.Value.Should().BeAssignableTo<ApiResponse<PagedResult<ProductSummaryResponse>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(pagedResult);
    }

    [Fact]
    public async Task SearchProducts_ShouldSendSearchProductsQuery_AndReturnOk()
    {
        // Arrange
        var request = new ProductSearchRequest(Keyword: "chuột không dây", Page: 1, Size: 20);
        var pagedResult = PagedResult<ProductSummaryResponse>.Create([], 1, 20, 0);

        _senderMock
            .Setup(s => s.Send(It.Is<SearchProductsQuery>(q => q.Request == request), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(pagedResult));

        // Act
        var result = await _controller.SearchProducts(request, CancellationToken.None);

        // Assert
        var objResult = result.Should().BeOfType<ObjectResult>().Subject;
        objResult.StatusCode.Should().Be(200);
        var response = objResult.Value.Should().BeAssignableTo<ApiResponse<PagedResult<ProductSummaryResponse>>>().Subject;
        response.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetProductDetail_ShouldSendGetPublicProductDetailQuery()
    {
        // Arrange
        var spuId = Guid.NewGuid();
        var spuDetail = new PublicSpuDetailResponse(
            Id: spuId,
            ShopId: Guid.NewGuid(),
            ShopName: "Shop A",
            CategoryId: Guid.NewGuid(),
            CategoryName: "Category A",
            Name: "Sample Product",
            Slug: "sample-product",
            Description: "Description",
            Brand: "Brand",
            ThumbnailUrl: "https://example.com/thumb.jpg",
            AttributesConfigJson: null,
            MinPrice: 100000m,
            MaxPrice: 200000m,
            Rating: 5.0,
            SoldCount: 0,
            Status: "Active",
            CreatedAt: DateTime.UtcNow,
            Skus: [],
            Attributes: []);

        _senderMock
            .Setup(s => s.Send(It.Is<GetPublicProductDetailQuery>(q => q.SpuId == spuId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(spuDetail));

        // Act
        var result = await _controller.GetProductDetail(spuId, CancellationToken.None);

        // Assert
        var objResult = result.Should().BeOfType<ObjectResult>().Subject;
        objResult.StatusCode.Should().Be(200);
        var response = objResult.Value.Should().BeAssignableTo<ApiResponse<PublicSpuDetailResponse>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.Id.Should().Be(spuId);
    }
}
