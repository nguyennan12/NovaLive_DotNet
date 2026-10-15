using FluentAssertions;
using Moq;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.UseCases.Categories.Queries.GetCategoriesTree;
using NovaLive.Contracts.V1.Categories;
using NovaLive.Domain.Products;
using Xunit;

namespace NovaLive.Application.Tests.Categories;

public class GetCategoriesTreeQueryHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock = new();
    private readonly Mock<ISpuRepository> _spuRepositoryMock = new();
    private readonly GetCategoriesTreeQueryHandler _handler;

    public GetCategoriesTreeQueryHandlerTests()
    {
        _handler = new GetCategoriesTreeQueryHandler(
            _categoryRepositoryMock.Object,
            _spuRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldBuildTreeAndRollupProductCounts()
    {
        // Arrange:
        // Root: Electronics (displayOrder: 1)
        //   -> Child: Phones (displayOrder: 1, 5 products)
        //   -> Child: Laptops (displayOrder: 2, 3 products)
        // Root: Fashion (displayOrder: 2, 2 products)
        var root1Id = Guid.NewGuid();
        var child1Id = Guid.NewGuid();
        var child2Id = Guid.NewGuid();
        var root2Id = Guid.NewGuid();

        var root1 = new Category(root1Id, "Electronics", "electronics", null, null, 1, true);
        var child1 = new Category(child1Id, "Phones", "phones", root1Id, null, 1, true);
        var child2 = new Category(child2Id, "Laptops", "laptops", root1Id, null, 2, true);
        var root2 = new Category(root2Id, "Fashion", "fashion", null, null, 2, true);

        var categories = new List<Category> { root1, child1, child2, root2 };

        _categoryRepositoryMock
            .Setup(r => r.GetAllAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        var productCounts = new Dictionary<Guid, int>
        {
            [child1Id] = 5,
            [child2Id] = 3,
            [root2Id] = 2
        };

        _spuRepositoryMock
            .Setup(r => r.GetProductCountsByCategoryIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(productCounts);

        var query = new GetCategoriesTreeQuery(IncludeInvisible: false);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var tree = result.Value;
        tree.Should().NotBeNull();
        tree!.Count.Should().Be(2);

        // First root: Electronics
        var firstRoot = tree[0];
        firstRoot.Id.Should().Be(root1Id);
        firstRoot.Name.Should().Be("Electronics");
        firstRoot.Children.Should().HaveCount(2);
        // Rollup product count: child1(5) + child2(3) = 8
        firstRoot.ProductCount.Should().Be(8);

        firstRoot.Children[0].Id.Should().Be(child1Id);
        firstRoot.Children[0].ProductCount.Should().Be(5);
        firstRoot.Children[1].Id.Should().Be(child2Id);
        firstRoot.Children[1].ProductCount.Should().Be(3);

        // Second root: Fashion
        var secondRoot = tree[1];
        secondRoot.Id.Should().Be(root2Id);
        secondRoot.ProductCount.Should().Be(2);
        secondRoot.Children.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenParentIsInvisible_ShouldNotPromoteChildToRootInPublicTree()
    {
        // Arrange: Parent is invisible (so omitted by repository when IncludeInvisible = false),
        // but child is visible with ParentId set.
        var invisibleParentId = Guid.NewGuid();
        var visibleChild = new Category(Guid.NewGuid(), "Phones", "phones", invisibleParentId, null, 1, true);
        var visibleRoot = new Category(Guid.NewGuid(), "Fashion", "fashion", null, null, 1, true);

        var categories = new List<Category> { visibleChild, visibleRoot };

        _categoryRepositoryMock
            .Setup(r => r.GetAllAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        _spuRepositoryMock
            .Setup(r => r.GetProductCountsByCategoryIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, int>());

        var query = new GetCategoriesTreeQuery(IncludeInvisible: false);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert: visibleChild should NOT be promoted to root.
        result.IsSuccess.Should().BeTrue();
        var tree = result.Value;
        tree.Should().HaveCount(1);
        tree![0].Id.Should().Be(visibleRoot.Id);
    }
}
