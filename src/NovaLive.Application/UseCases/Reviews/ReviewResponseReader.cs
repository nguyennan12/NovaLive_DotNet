using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Contracts.V1.Reviews;
using NovaLive.Domain.Reviews;

namespace NovaLive.Application.UseCases.Reviews;

public sealed class ReviewResponseReader(ISkuRepository skus, IUserRepository users)
{
    public async Task<ReviewResponse> ReadAsync(Review review, CancellationToken ct)
    {
        var sku = await skus.GetByIdAsync(review.SkuId, ct);
        var buyer = await users.GetByIdAsync(review.BuyerId, ct);
        return review.ToResponse(sku, buyer);
    }
}
