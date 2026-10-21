using FluentAssertions;
using Moq;
using NovaLive.Application.UseCases.Reviews.Queries.GetProductReviews;
using NovaLive.Domain.Products;
using NovaLive.Domain.Reviews;

namespace NovaLive.Application.Tests.Reviews;

public sealed class GetProductReviewsTests
{
    [Fact]
    public async Task NoSkus_ReturnsEmptyPageWithoutQueryingReviews()
    {
        var c = new ReviewTestContext();
        c.Skus.Setup(s => s.GetBySpuIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync([]);
        var result = await new GetProductReviewsQueryHandler(c.Skus.Object, c.Reviews.Object, c.Users.Object)
            .Handle(new(Guid.NewGuid(), Page: 2, Size: 10), default);
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().BeEmpty();
        result.Value.Total.Should().Be(0);
        result.Value.Page.Should().Be(2);
        c.Reviews.Verify(r => r.GetVisiblePagedAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<int?>(),
            It.IsAny<bool?>(), It.IsAny<int>(), It.IsAny<int>(), default), Times.Never());
    }

    [Theory]
    [InlineData(null, null, 1, 20)]
    [InlineData(5, true, 2, 10)]
    [InlineData(1, false, 3, 5)]
    public async Task Query_PassesAllProductSkuIdsAndFiltersToVisibleRepositoryAndPreservesPagination(
        int? rating, bool? hasMedia, int page, int size)
    {
        var c = new ReviewTestContext();
        var second = new Sku(c.Sku.SpuId, c.Shop.Id, "SECOND", "{}", 100, 100);
        c.Skus.Setup(s => s.GetBySpuIdAsync(c.Sku.SpuId, default)).ReturnsAsync([c.Sku, second]);
        c.Reviews.Setup(r => r.GetVisiblePagedAsync(
            It.Is<IReadOnlyCollection<Guid>>(ids => ids.Count == 2 && ids.Contains(c.Sku.Id) && ids.Contains(second.Id)),
            rating, hasMedia, page, size, default)).ReturnsAsync((new List<Review> { c.Review }, 31L));
        var result = await new GetProductReviewsQueryHandler(c.Skus.Object, c.Reviews.Object, c.Users.Object)
            .Handle(new(c.Sku.SpuId, rating, hasMedia, page, size), default);
        result.IsSuccess.Should().BeTrue();
        result.Value!.Total.Should().Be(31);
        result.Value.Page.Should().Be(page);
        result.Value.Size.Should().Be(size);
        result.Value.Items.Should().ContainSingle().Which.SpuId.Should().Be(c.Sku.SpuId);
        result.Value.Items.Single().BuyerName.Should().Be("N***");
        c.Reviews.Verify(r => r.GetVisiblePagedAsync(It.IsAny<IReadOnlyCollection<Guid>>(),
            rating, hasMedia, page, size, default), Times.Once());
    }
}
