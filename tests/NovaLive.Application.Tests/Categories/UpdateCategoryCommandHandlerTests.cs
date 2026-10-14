using FluentAssertions;
using Moq;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.UseCases.Categories;
using NovaLive.Application.UseCases.Categories.Commands.UpdateCategory;
using NovaLive.Contracts.V1.Categories;
using NovaLive.Domain.Products;
using Xunit;

namespace NovaLive.Application.Tests.Categories;

public class UpdateCategoryCommandHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock = new();
    private readonly Mock<ISpuRepository> _spuRepositoryMock = new();
    private readonly UpdateCategoryCommandHandler _handler;

    public UpdateCategoryCommandHandlerTests()
    {
        _handler = new UpdateCategoryCommandHandler(
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

        var request = new UpdateCategoryRequest(
            Name: "Test",
            ParentId: null,
            IconUrl: null,
            DisplayOrder: 1,
            IsVisible: true,
            Slug: "test");

        var result = await _handler.Handle(new UpdateCategoryCommand(catId, request), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CategoryErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WhenParentIsSelf_ShouldReturnCannotBeOwnParent()
    {
        var catId = Guid.NewGuid();
        var category = new Category(catId, "Test", "test", null, null, 1, true);

        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(catId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var request = new UpdateCategoryRequest(
            Name: "Test",
            ParentId: catId, // Self parent
            IconUrl: null,
            DisplayOrder: 1,
            IsVisible: true,
            Slug: "test");

        var result = await _handler.Handle(new UpdateCategoryCommand(catId, request), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CategoryErrors.CannotBeOwnParent);
    }

    [Fact]
    public async Task Handle_WhenCircularReferenceDetected_ShouldReturnCircularReference()
    {
        // Hierarchy: A (catId) -> B -> C
        // Updating A's parent to C -> cycle!
        var catId = Guid.NewGuid();
        var bId = Guid.NewGuid();
        var cId = Guid.NewGuid();

        var catA = new Category(catId, "Category A", "category-a", null, null, 1, true);
        var catB = new Category(bId, "Category B", "category-b", catId, null, 1, true);
        var catC = new Category(cId, "Category C", "category-c", bId, null, 1, true);

        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(catId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(catA);

        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(cId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(catC);

        _categoryRepositoryMock
            .Setup(r => r.GetAllAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category> { catA, catB, catC });

        var request = new UpdateCategoryRequest(
            Name: "Category A Modified",
            ParentId: cId, // Setting C as parent of A
            IconUrl: null,
            DisplayOrder: 1,
            IsVisible: true,
            Slug: "category-a-modified");

        var result = await _handler.Handle(new UpdateCategoryCommand(catId, request), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CategoryErrors.CircularReference);
        _categoryRepositoryMock.Verify(r => r.Update(It.IsAny<Category>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenValidUpdate_ShouldUpdateSuccessfully()
    {
        var catId = Guid.NewGuid();
        var category = new Category(catId, "Old Name", "old-slug", null, null, 1, true);

        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(catId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _categoryRepositoryMock
            .Setup(r => r.GetBySlugAsync("new-slug", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        _spuRepositoryMock
            .Setup(r => r.GetProductCountsByCategoryIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, int> { [catId] = 4 });

        var request = new UpdateCategoryRequest(
            Name: "New Name",
            ParentId: null,
            IconUrl: "https://icon.png",
            DisplayOrder: 10,
            IsVisible: false,
            Slug: "new-slug");

        var result = await _handler.Handle(new UpdateCategoryCommand(catId, request), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be("New Name");
        result.Value.Slug.Should().Be("new-slug");
        result.Value.IsVisible.Should().BeFalse();
        result.Value.ProductCount.Should().Be(4);

        _categoryRepositoryMock.Verify(r => r.Update(category), Times.Once);
    }
}
