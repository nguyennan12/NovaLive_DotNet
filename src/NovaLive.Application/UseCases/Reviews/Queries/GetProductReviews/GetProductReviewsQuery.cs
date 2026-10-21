using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Reviews;
using NovaLive.Domain.Reviews;

namespace NovaLive.Application.UseCases.Reviews.Queries.GetProductReviews;

public sealed record GetProductReviewsQuery(
    Guid SpuId, int? Rating = null, bool? HasMedia = null, int Page = 1, int Size = ReviewLimits.DefaultPageSize)
    : IQuery<PagedResult<ReviewResponse>>;
