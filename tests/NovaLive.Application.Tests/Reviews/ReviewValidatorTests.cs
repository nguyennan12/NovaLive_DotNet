using FluentAssertions;
using Microsoft.Extensions.Options;
using NovaLive.Application.UseCases.Reviews;
using NovaLive.Application.UseCases.Reviews.Commands.CreateReview;
using NovaLive.Application.UseCases.Reviews.Commands.UpdateReview;
using NovaLive.Application.UseCases.Reviews.Commands.ReplyReview;
using NovaLive.Application.UseCases.Reviews.Commands.HideReview;
using NovaLive.Application.UseCases.Reviews.Queries.GetProductReviews;
using NovaLive.Contracts.V1.Reviews;
using NovaLive.Domain.Reviews;

namespace NovaLive.Application.Tests.Reviews;

public sealed class ReviewValidatorTests
{
    private static readonly IOptions<ReviewMediaOptions> OptionsValue = Options.Create(new ReviewMediaOptions
        { BucketBaseUrl = "https://cdn.example.com/reviews/" });
    private static CreateReviewRequest Valid => new(Guid.NewGuid(), 5, "Title", "Content", ["https://cdn.example.com/reviews/a.JPG?version=1"]);

    [Fact]
    public void CreateAndUpdate_AcceptLimitsAndOptionalFields()
    {
        var create = new CreateReviewCommandValidator(OptionsValue);
        var update = new UpdateReviewCommandValidator(OptionsValue);
        foreach (var rating in new[] { 1, 5 })
        {
            var request = Valid with { Rating = rating, Title = new('t', ReviewLimits.TitleLength),
                Content = new('c', ReviewLimits.ContentLength), MediaUrls = Enumerable.Repeat("https://cdn.example.com/reviews/a.mp4", ReviewLimits.MediaCount).ToList() };
            create.Validate(new CreateReviewCommand(request)).IsValid.Should().BeTrue();
            update.Validate(new UpdateReviewCommand(Guid.NewGuid(), new(request.Rating, request.Title, request.Content, request.MediaUrls))).IsValid.Should().BeTrue();
        }
        create.Validate(new CreateReviewCommand(Valid with { Title = null, Content = null, MediaUrls = null })).IsValid.Should().BeTrue();
        update.Validate(new UpdateReviewCommand(Guid.NewGuid(), new(1, null, null, null))).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("rating-low")]
    [InlineData("rating-high")]
    [InlineData("title")]
    [InlineData("content")]
    [InlineData("count")]
    [InlineData("http")]
    [InlineData("relative")]
    [InlineData("extension")]
    [InlineData("host")]
    [InlineData("bucket")]
    [InlineData("bucket-prefix")]
    [InlineData("credentials")]
    [InlineData("encoded-path")]
    [InlineData("url-length")]
    [InlineData("null-url")]
    public void CreateAndUpdate_RejectInvalidContent(string scenario)
    {
        var request = scenario switch
        {
            "rating-low" => Valid with { Rating = 0 },
            "rating-high" => Valid with { Rating = 6 },
            "title" => Valid with { Title = new('t', ReviewLimits.TitleLength + 1) },
            "content" => Valid with { Content = new('c', ReviewLimits.ContentLength + 1) },
            "count" => Valid with { MediaUrls = Enumerable.Repeat("https://cdn.example.com/reviews/a.jpg", ReviewLimits.MediaCount + 1).ToList() },
            "http" => Valid with { MediaUrls = ["http://cdn.example.com/reviews/a.jpg"] },
            "relative" => Valid with { MediaUrls = ["/reviews/a.jpg"] },
            "extension" => Valid with { MediaUrls = ["https://cdn.example.com/reviews/a.svg"] },
            "host" => Valid with { MediaUrls = ["https://cdn.example.com.attacker.test/reviews/a.jpg"] },
            "bucket" => Valid with { MediaUrls = ["https://cdn.example.com/products/a.jpg"] },
            "bucket-prefix" => Valid with { MediaUrls = ["https://cdn.example.com/reviews-evil/a.jpg"] },
            "credentials" => Valid with { MediaUrls = ["https://user:password@cdn.example.com/reviews/a.jpg"] },
            "encoded-path" => Valid with { MediaUrls = ["https://cdn.example.com/reviews/%2fproducts/a.jpg"] },
            "url-length" => Valid with { MediaUrls = ["https://cdn.example.com/reviews/" + new string('a', ReviewLimits.MediaUrlLength) + ".jpg"] },
            _ => Valid with { MediaUrls = [null!] }
        };
        new CreateReviewCommandValidator(OptionsValue).Validate(new CreateReviewCommand(request)).IsValid.Should().BeFalse();
        new UpdateReviewCommandValidator(OptionsValue)
            .Validate(new UpdateReviewCommand(Guid.NewGuid(), new(request.Rating, request.Title, request.Content, request.MediaUrls))).IsValid.Should().BeFalse();
    }

    [Fact]
    public void NoCdnConfiguration_AcceptsValidHttpsMediaFromAnyHost()
    {
        var validator = new CreateReviewCommandValidator(Options.Create(new ReviewMediaOptions()));
        validator.Validate(new CreateReviewCommand(Valid with { MediaUrls = ["https://another.example/review.webp"] })).IsValid.Should().BeTrue();
    }

    [Fact]
    public void AllValidators_RejectEmptyIdsAndRequests()
    {
        new CreateReviewCommandValidator(OptionsValue).Validate(new CreateReviewCommand(Valid with { OrderItemId = Guid.Empty })).IsValid.Should().BeFalse();
        new CreateReviewCommandValidator(OptionsValue).Validate(new CreateReviewCommand(null!)).IsValid.Should().BeFalse();
        new UpdateReviewCommandValidator(OptionsValue).Validate(new UpdateReviewCommand(Guid.Empty, new(1, null, null, null))).IsValid.Should().BeFalse();
        new UpdateReviewCommandValidator(OptionsValue).Validate(new UpdateReviewCommand(Guid.NewGuid(), null!)).IsValid.Should().BeFalse();
        new ReplyReviewCommandValidator().Validate(new ReplyReviewCommand(Guid.Empty, new("Reply"))).IsValid.Should().BeFalse();
        new ReplyReviewCommandValidator().Validate(new ReplyReviewCommand(Guid.NewGuid(), null!)).IsValid.Should().BeFalse();
        new HideReviewCommandValidator().Validate(new HideReviewCommand(Guid.Empty, new("Reason"))).IsValid.Should().BeFalse();
        new HideReviewCommandValidator().Validate(new HideReviewCommand(Guid.NewGuid(), null!)).IsValid.Should().BeFalse();
        new GetProductReviewsQueryValidator().Validate(new GetProductReviewsQuery(Guid.Empty)).IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void ReplyAndHide_RejectBlankText(string text)
    {
        new ReplyReviewCommandValidator().Validate(new ReplyReviewCommand(Guid.NewGuid(), new(text))).IsValid.Should().BeFalse();
        new HideReviewCommandValidator().Validate(new HideReviewCommand(Guid.NewGuid(), new(text))).IsValid.Should().BeFalse();
    }

    [Fact]
    public void ReplyAndHide_RespectLengthLimits()
    {
        var reply = new ReplyReviewCommandValidator();
        var hide = new HideReviewCommandValidator();
        reply.Validate(new ReplyReviewCommand(Guid.NewGuid(), new(new('r', ReviewLimits.ReplyLength)))).IsValid.Should().BeTrue();
        reply.Validate(new ReplyReviewCommand(Guid.NewGuid(), new(new('r', ReviewLimits.ReplyLength + 1)))).IsValid.Should().BeFalse();
        hide.Validate(new HideReviewCommand(Guid.NewGuid(), new(new('r', ReviewLimits.ReasonLength)))).IsValid.Should().BeTrue();
        hide.Validate(new HideReviewCommand(Guid.NewGuid(), new(new('r', ReviewLimits.ReasonLength + 1)))).IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(0, 20, null)]
    [InlineData(1, 0, null)]
    [InlineData(1, 51, null)]
    [InlineData(1, 20, 0)]
    [InlineData(1, 20, 6)]
    [InlineData(int.MaxValue, 50, null)]
    public void Query_RejectsInvalidPaginationAndRating(int page, int size, int? rating) =>
        new GetProductReviewsQueryValidator().Validate(new GetProductReviewsQuery(Guid.NewGuid(), rating, null, page, size)).IsValid.Should().BeFalse();
}
