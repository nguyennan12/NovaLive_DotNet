using FluentAssertions;
using Moq;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.UseCases.Categories;
using NovaLive.Application.UseCases.Categories.Commands.CreateCategory;
using NovaLive.Contracts.V1.Categories;
using NovaLive.Domain.Products;
using Xunit;

namespace NovaLive.Application.Tests.Categories;

public class CreateCategoryCommandHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock = new();
    private readonly CreateCategoryCommandHandler _handler;

    public CreateCategoryCommandHandlerTests()
    {
        _handler = new CreateCategoryCommandHandler(_categoryRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenParentDoesNotExist_ShouldReturnParentNotFound()
    {
        var parentId = Guid.NewGuid();
        _categoryRepositoryMock
            .Setup(r => r.ExistsAsync(parentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var request = new CreateCategoryRequest(
            Name: "Smartphones",
            ParentId: parentId,
            IconUrl: null,
            DisplayOrder: 1,
            Slug: "smartphones");

        var result = await _handler.Handle(new CreateCategoryCommand(request), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CategoryErrors.ParentNotFound);
        _categoryRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenSlugAlreadyExists_ShouldReturnSlugAlreadyExists()
    {
        var existingCat = new Category(Guid.NewGuid(), "Old Cat", "smartphones", null, null, 1, true);
        _categoryRepositoryMock
            .Setup(r => r.GetBySlugAsync("smartphones", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCat);

        var request = new CreateCategoryRequest(
            Name: "Smartphones",
            ParentId: null,
            IconUrl: null,
            DisplayOrder: 1,
            Slug: "smartphones");

        var result = await _handler.Handle(new CreateCategoryCommand(request), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CategoryErrors.SlugAlreadyExists);
        _categoryRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenValidWithoutSlug_ShouldGenerateSlugAndCreate()
    {
        _categoryRepositoryMock
            .Setup(r => r.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var request = new CreateCategoryRequest(
            Name: "Đồ Gia Dụng Thông Minh",
            ParentId: null,
            IconUrl: "https://cdn.novalive.vn/products/icon.png",
            DisplayOrder: 2,
            Slug: null);

        var result = await _handler.Handle(new CreateCategoryCommand(request), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be("Đồ Gia Dụng Thông Minh");
        result.Value.Slug.Should().Be("do-gia-dung-thong-minh");
        result.Value.DisplayOrder.Should().Be(2);

        _categoryRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
