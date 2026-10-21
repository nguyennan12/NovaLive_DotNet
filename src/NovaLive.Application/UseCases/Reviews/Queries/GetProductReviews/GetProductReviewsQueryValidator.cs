using FluentValidation;
using NovaLive.Domain.Reviews;

namespace NovaLive.Application.UseCases.Reviews.Queries.GetProductReviews;

public sealed class GetProductReviewsQueryValidator : AbstractValidator<GetProductReviewsQuery>
{
    public GetProductReviewsQueryValidator()
    {
        RuleFor(x => x.SpuId).NotEmpty();
        RuleFor(x => x.Rating).InclusiveBetween(ReviewLimits.MinRating, ReviewLimits.MaxRating).When(x => x.Rating.HasValue);
        RuleFor(x => x.Page).GreaterThan(0)
            .Must((query, page) => (long)(page - 1) * query.Size <= int.MaxValue);
        RuleFor(x => x.Size).InclusiveBetween(1, ReviewLimits.MaxPageSize);
    }
}
