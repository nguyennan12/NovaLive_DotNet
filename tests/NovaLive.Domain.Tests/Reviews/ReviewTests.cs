using FluentAssertions;
using NovaLive.Domain.Common;
using NovaLive.Domain.Reviews;

namespace NovaLive.Domain.Tests.Reviews;

public sealed class ReviewTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 1, 0, 0, 0, TimeSpan.Zero);
    private static Result<Review> Create(int rating = 5, List<string>? media = null) =>
        Review.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), rating, "Title", "Content", media, Now);

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void Create_InitializesAggregateAndMedia(int rating)
    {
        var review = Create(rating, ["https://cdn.example.com/reviews/a.jpg", "https://cdn.example.com/reviews/b.mp4"]).Value!;
        review.Rating.Should().Be((short)rating);
        review.EditCount.Should().Be(0);
        review.IsVisible.Should().BeTrue();
        review.CreatedAt.Should().Be(Now);
        review.UpdatedAt.Should().Be(Now);
        review.Images.Select(i => i.MediaType).Should().Equal(ReviewMediaType.image, ReviewMediaType.video);
        review.Images.Should().OnlyContain(i => i.ReviewId == review.Id && i.CreatedAt == Now);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void Create_RejectsRatingOutsideBounds(int rating) => Create(rating).Error.Should().Be(ReviewErrors.InvalidRating);

    [Fact]
    public void Update_ReplacesMediaOnceAndStampsUpdate()
    {
        var review = Create(media: ["https://cdn.example.com/old.png"]).Value!;
        var oldId = review.Images.Single().Id;
        review.Update(1, "New", "Changed", ["https://cdn.example.com/new.mp4"], Now.AddDays(1)).IsSuccess.Should().BeTrue();
        review.Rating.Should().Be(1);
        review.EditCount.Should().Be(1);
        review.Images.Should().ContainSingle().Which.Id.Should().NotBe(oldId);
        review.Images.Single().MediaType.Should().Be(ReviewMediaType.video);
        review.UpdatedAt.Should().Be(Now.AddDays(1));
        review.Update(2, null, null, [], Now.AddDays(2)).Error.Should().Be(ReviewErrors.AlreadyEdited);
        review.Rating.Should().Be(1);
    }

    [Fact]
    public void Update_AtExactlyThirtyDaysIsAllowed() =>
        Create().Value!.Update(3, null, null, null, Now.AddDays(30)).IsSuccess.Should().BeTrue();

    [Fact]
    public void Update_OneTickPastThirtyDaysIsRejected()
    {
        var review = Create().Value!;
        review.Update(3, null, null, null, Now.AddDays(30).AddTicks(1)).Error.Should().Be(ReviewErrors.EditExpired);
        review.EditCount.Should().Be(0);
    }

    [Fact]
    public void HiddenReview_CannotBeEditedButCanBeRepliedTo()
    {
        var review = Create().Value!;
        review.Hide("Spam", Now.AddDays(1)).IsSuccess.Should().BeTrue();
        review.Update(1, null, null, null, Now.AddDays(2)).Error.Should().Be(ReviewErrors.Hidden);
        review.Reply("Thanks", Now.AddDays(2)).IsSuccess.Should().BeTrue();
        review.SellerReply.Should().Be("Thanks");
        review.SellerRepliedAt.Should().Be(Now.AddDays(2));
        review.Reply("Again", Now.AddDays(3)).Error.Should().Be(ReviewErrors.AlreadyReplied);
    }

    [Fact]
    public void Hide_IsIdempotentAndDoesNotRestampAnAlreadyHiddenReview()
    {
        var review = Create().Value!;
        review.Hide("Spam", Now.AddDays(1)).IsSuccess.Should().BeTrue();
        review.Hide("Spam again", Now.AddDays(2)).IsSuccess.Should().BeTrue();
        review.IsVisible.Should().BeFalse();
        review.UpdatedAt.Should().Be(Now.AddDays(1));
    }

    [Theory]
    [InlineData(SubOrderStatus.Delivered, OrderItemStatus.Active, true)]
    [InlineData(SubOrderStatus.Completed, OrderItemStatus.Active, true)]
    [InlineData(SubOrderStatus.Delivered, OrderItemStatus.PartialReturn, true)]
    [InlineData(SubOrderStatus.Delivered, OrderItemStatus.Returned, false)]
    [InlineData(SubOrderStatus.Shipping, OrderItemStatus.Active, false)]
    [InlineData(SubOrderStatus.Cancelled, OrderItemStatus.Active, false)]
    [InlineData(SubOrderStatus.ReturnRequested, OrderItemStatus.Active, false)]
    [InlineData(SubOrderStatus.Returned, OrderItemStatus.Active, false)]
    [InlineData(SubOrderStatus.Refunded, OrderItemStatus.Active, false)]
    public void PurchaseEligibility_IsEnforcedInDomain(SubOrderStatus status, OrderItemStatus itemStatus, bool eligible) =>
        Review.ValidatePurchase(status, itemStatus).IsSuccess.Should().Be(eligible);

    [Fact]
    public void InvalidUpdate_IsAtomic()
    {
        var review = Create(media: ["https://cdn.example.com/a.png"]).Value!;
        review.Update(0, "Changed", null, [], Now.AddDays(1)).Error.Should().Be(ReviewErrors.InvalidRating);
        review.EditCount.Should().Be(0);
        review.Title.Should().Be("Title");
        review.Images.Should().ContainSingle();
    }

    [Fact]
    public void PersistenceHydration_CannotReplaceMediaOfAnInitializedAggregate()
    {
        var review = Create(media: ["https://cdn.example.com/a.png"]).Value!;
        review.RestoreImages([]).Error.Should().Be(ReviewErrors.InvalidMedia);
        review.Images.Should().ContainSingle();
        review.EditCount.Should().Be(0);
    }

    [Fact]
    public void Aggregate_ExposesNoPublicPropertySetters()
    {
        typeof(Review).GetProperties().Should().OnlyContain(p => p.SetMethod == null || !p.SetMethod.IsPublic);
        typeof(ReviewImage).GetProperties().Should().OnlyContain(p => p.SetMethod == null || !p.SetMethod.IsPublic);
    }
}
