using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Reviews;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Reviews.Queries.GetProductReviews;

public sealed class GetProductReviewsQueryHandler(ISkuRepository skus, IReviewRepository reviews, IUserRepository users)
    : IQueryHandler<GetProductReviewsQuery, PagedResult<ReviewResponse>>
{
    public async Task<Result<PagedResult<ReviewResponse>>> Handle(GetProductReviewsQuery query, CancellationToken ct)
    {
        var productSkus = await skus.GetBySpuIdAsync(query.SpuId, ct);
        if (productSkus.Count == 0)
            return PagedResult<ReviewResponse>.Create([], query.Page, query.Size, 0);
        var byId = productSkus.ToDictionary(sku => sku.Id);
        var (items, total) = await reviews.GetVisiblePagedAsync(byId.Keys.ToArray(),
            query.Rating, query.HasMedia, query.Page, query.Size, ct);
        var buyers = (await users.GetByIdsAsync(items.Select(r => r.BuyerId).Distinct(), ct)).ToDictionary(u => u.Id);
        var responses = items.Select(r => r.ToResponse(byId.GetValueOrDefault(r.SkuId),
            buyers.GetValueOrDefault(r.BuyerId))).ToList();
        return PagedResult<ReviewResponse>.Create(responses, query.Page, query.Size, total);
    }
}
