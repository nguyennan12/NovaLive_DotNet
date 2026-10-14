using FluentAssertions;
using Moq;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.UseCases.Categories;
using NovaLive.Application.UseCases.Categories.Commands.DeleteCategory;
using NovaLive.Domain.Common;
using NovaLive.Domain.Products;
using Xunit;

namespace NovaLive.Application.Tests.Categories;

public class DeleteCategoryCommandHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock = new();
    private readonly Mock<ISpuRepository> _spuRepositoryMock = new();
    private readonly DeleteCategoryCommandHandler _handler;

    public DeleteCategoryCommandHandlerTests()
    {
        _handler = new DeleteCategoryCommandHandler(
            _categoryRepositoryMock.Object,
            _spuRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCategoryNotFound_ShouldReturnNotFound()
    {
        var catId = Guid.NewGuid();
        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(catId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var result = await _handler.Handle(new DeleteCategoryCommand(catId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CategoryErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WhenCategoryHasChildren_ShouldReturnCannotDeleteError()
    {
        var catId = Guid.NewGuid();
        var category = new Category(catId, "Parent Cat", "parent-cat", null, null, 1, true);

        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(catId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _categoryRepositoryMock
            .Setup(r => r.HasChildrenAsync(catId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(new DeleteCategoryCommand(catId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CategoryErrors.CannotDeleteCategoryWithChildrenOrProducts);
        result.Error.Type.Should().Be(ErrorType.Validation); // Maps to 400 Bad Request
        _categoryRepositoryMock.Verify(r => r.Delete(It.IsAny<Category>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCategoryHasProducts_ShouldReturnCannotDeleteError()
    {
        var catId = Guid.NewGuid();
        var category = new Category(catId, "Leaf Cat", "leaf-cat", null, null, 1, true);

        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(catId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _categoryRepositoryMock
            .Setup(r => r.HasChildrenAsync(catId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _spuRepositoryMock
            .Setup(r => r.ExistsByCategoryIdAsync(catId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(new DeleteCategoryCommand(catId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CategoryErrors.CannotDeleteCategoryWithChildrenOrProducts);
        result.Error.Type.Should().Be(ErrorType.Validation); // Maps to 400 Bad Request
        _categoryRepositoryMock.Verify(r => r.Delete(It.IsAny<Category>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCategoryHasNoChildrenAndNoProducts_ShouldDeleteSuccessfully()
    {
        var catId = Guid.NewGuid();
        var category = new Category(catId, "Leaf Cat", "leaf-cat", null, null, 1, true);

        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(catId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _categoryRepositoryMock
            .Setup(r => r.HasChildrenAsync(catId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _spuRepositoryMock
            .Setup(r => r.ExistsByCategoryIdAsync(catId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.Handle(new DeleteCategoryCommand(catId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _categoryRepositoryMock.Verify(r => r.Delete(category), Times.Once);
    }
}
