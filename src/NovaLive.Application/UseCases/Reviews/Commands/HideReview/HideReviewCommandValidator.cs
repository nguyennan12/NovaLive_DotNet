using FluentValidation;
using NovaLive.Domain.Reviews;

namespace NovaLive.Application.UseCases.Reviews.Commands.HideReview;

public sealed class HideReviewCommandValidator : AbstractValidator<HideReviewCommand>
{
    public HideReviewCommandValidator()
    {
        RuleFor(x => x.ReviewId).NotEmpty();
        RuleFor(x => x.Request).NotNull();
        When(x => x.Request is not null, () =>
        {
            RuleFor(x => x.Request.Reason).NotEmpty().MaximumLength(ReviewLimits.ReasonLength);
        });
    }
}
