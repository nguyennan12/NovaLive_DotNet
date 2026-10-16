using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NovaLive.Api.Controllers.Categories;
using NovaLive.Application.UseCases.Categories;
using NovaLive.Application.UseCases.Categories.Queries.GetCategoriesTree;
using NovaLive.Application.UseCases.Categories.Queries.GetCategoryById;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Categories;
using NovaLive.Domain.Common;
using Xunit;

namespace NovaLive.Api.Tests.Categories;

public class CategoriesControllerTests
{
    private readonly Mock<ISender> _senderMock = new();
    private readonly CategoriesController _controller;

    public CategoriesControllerTests()
    {
        _controller = new CategoriesController(_senderMock.Object);
    }

    [Fact]
    public async Task GetCategoriesTree_ShouldReturnOkWithTree()
    {
        // Arrange
        var tree = new List<CategoryNodeDto>
        {
            new(Guid.NewGuid(), "Electronics", "electronics", null, null, 1, 10, [])
        };

        _senderMock
            .Setup(s => s.Send(It.IsAny<GetCategoriesTreeQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<IReadOnlyList<CategoryNodeDto>>(tree));

        // Act
        var result = await _controller.GetCategoriesTree(CancellationToken.None);

        // Assert
        var objResult = result.Should().BeOfType<ObjectResult>().Subject;
        objResult.StatusCode.Should().Be(200);
        var response = objResult.Value.Should().BeAssignableTo<ApiResponse<IReadOnlyList<CategoryNodeDto>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetCategoryById_WhenFound_ShouldReturnOk()
    {
        // Arrange
        var catId = Guid.NewGuid();
        var catResponse = new CategoryResponse(catId, "Electronics", "electronics", null, null, 1, true, 5);

        _senderMock
            .Setup(s => s.Send(It.Is<GetCategoryByIdQuery>(q => q.Id == catId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(catResponse));

        // Act
        var result = await _controller.GetCategoryById(catId, CancellationToken.None);

        // Assert
        var objResult = result.Should().BeOfType<ObjectResult>().Subject;
        objResult.StatusCode.Should().Be(200);
        var response = objResult.Value.Should().BeAssignableTo<ApiResponse<CategoryResponse>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.Id.Should().Be(catId);
    }

    [Fact]
    public async Task GetCategoryById_WhenNotFound_ShouldReturn404()
    {
        // Arrange
        var catId = Guid.NewGuid();

        _senderMock
            .Setup(s => s.Send(It.Is<GetCategoryByIdQuery>(q => q.Id == catId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<CategoryResponse>(CategoryErrors.NotFound));

        // Act
        var result = await _controller.GetCategoryById(catId, CancellationToken.None);

        // Assert
        var objResult = result.Should().BeOfType<ObjectResult>().Subject;
        objResult.StatusCode.Should().Be(404);
    }
}
