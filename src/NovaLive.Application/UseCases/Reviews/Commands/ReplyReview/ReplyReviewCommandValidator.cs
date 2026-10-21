using FluentValidation;
using NovaLive.Domain.Reviews;

namespace NovaLive.Application.UseCases.Reviews.Commands.ReplyReview;

public sealed class ReplyReviewCommandValidator : AbstractValidator<ReplyReviewCommand>
{
    public ReplyReviewCommandValidator()
    {
        RuleFor(x => x.ReviewId).NotEmpty();
        RuleFor(x => x.Request).NotNull();
        When(x => x.Request is not null, () =>
        {
            RuleFor(x => x.Request.ReplyText).NotEmpty().MaximumLength(ReviewLimits.ReplyLength);
        });
    }
}
