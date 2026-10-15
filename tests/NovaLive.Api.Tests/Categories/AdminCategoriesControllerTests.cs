using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NovaLive.Api.Controllers.Categories;
using NovaLive.Application.UseCases.Categories;
using NovaLive.Application.UseCases.Categories.Commands.CreateCategory;
using NovaLive.Application.UseCases.Categories.Commands.DeleteCategory;
using NovaLive.Application.UseCases.Categories.Commands.UpdateCategory;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Categories;
using NovaLive.Domain.Common;
using Xunit;

namespace NovaLive.Api.Tests.Categories;

public class AdminCategoriesControllerTests
{
    private readonly Mock<ISender> _senderMock = new();
    private readonly AdminCategoriesController _controller;

    public AdminCategoriesControllerTests()
    {
        _controller = new AdminCategoriesController(_senderMock.Object);
    }

    [Fact]
    public async Task CreateCategory_WhenValid_ShouldReturn201Created()
    {
        // Arrange
        var request = new CreateCategoryRequest("Laptops", null, null, 1, "laptops");
        var created = new CategoryResponse(Guid.NewGuid(), "Laptops", "laptops", null, null, 1, true, 0);

        _senderMock
            .Setup(s => s.Send(It.IsAny<CreateCategoryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(created));

        // Act
        var result = await _controller.CreateCategory(request, CancellationToken.None);

        // Assert
        var objResult = result.Should().BeOfType<ObjectResult>().Subject;
        objResult.StatusCode.Should().Be(201);
        var response = objResult.Value.Should().BeAssignableTo<ApiResponse<CategoryResponse>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.Name.Should().Be("Laptops");
    }

    [Fact]
    public async Task UpdateCategory_WhenValid_ShouldReturn200Ok()
    {
        // Arrange
        var catId = Guid.NewGuid();
        var request = new UpdateCategoryRequest("Updated", null, null, 1, true, "updated");
        var updated = new CategoryResponse(catId, "Updated", "updated", null, null, 1, true, 0);

        _senderMock
            .Setup(s => s.Send(It.Is<UpdateCategoryCommand>(c => c.Id == catId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(updated));

        // Act
        var result = await _controller.UpdateCategory(catId, request, CancellationToken.None);

        // Assert
        var objResult = result.Should().BeOfType<ObjectResult>().Subject;
        objResult.StatusCode.Should().Be(200);
        var response = objResult.Value.Should().BeAssignableTo<ApiResponse<CategoryResponse>>().Subject;
        response.Success.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteCategory_WhenHasChildren_ShouldReturn400BadRequest()
    {
        // Arrange
        var catId = Guid.NewGuid();

        _senderMock
            .Setup(s => s.Send(It.Is<DeleteCategoryCommand>(c => c.Id == catId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(CategoryErrors.CannotDeleteCategoryWithChildrenOrProducts));

        // Act
        var result = await _controller.DeleteCategory(catId, CancellationToken.None);

        // Assert
        var objResult = result.Should().BeOfType<ObjectResult>().Subject;
        objResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task DeleteCategory_WhenSuccess_ShouldReturn200Ok()
    {
        // Arrange
        var catId = Guid.NewGuid();

        _senderMock
            .Setup(s => s.Send(It.Is<DeleteCategoryCommand>(c => c.Id == catId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _controller.DeleteCategory(catId, CancellationToken.None);

        // Assert
        var objResult = result.Should().BeOfType<ObjectResult>().Subject;
        objResult.StatusCode.Should().Be(200);
        var response = objResult.Value.Should().BeAssignableTo<ApiResponse<object>>().Subject;
        response.Success.Should().BeTrue();
    }
}
