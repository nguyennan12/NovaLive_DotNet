using FluentValidation;
using NovaLive.Domain.Reviews;
using Microsoft.Extensions.Options;

namespace NovaLive.Application.UseCases.Reviews.Commands.CreateReview;

public sealed class CreateReviewCommandValidator : AbstractValidator<CreateReviewCommand>
{
    public CreateReviewCommandValidator(IOptions<ReviewMediaOptions> options)
    {
        RuleFor(x => x.Request).NotNull();
        When(x => x.Request is not null, () =>
        {
            RuleFor(x => x.Request.OrderItemId).NotEmpty();
            RuleFor(x => x.Request.Rating).InclusiveBetween(ReviewLimits.MinRating, ReviewLimits.MaxRating);
            RuleFor(x => x.Request.Title).MaximumLength(ReviewLimits.TitleLength);
            RuleFor(x => x.Request.Content).MaximumLength(ReviewLimits.ContentLength);
            RuleFor(x => x.Request.MediaUrls).Must(urls => urls is null || urls.Count <= ReviewLimits.MediaCount);
            RuleForEach(x => x.Request.MediaUrls).Must(options.Value.IsAllowed);
        });
    }
}
