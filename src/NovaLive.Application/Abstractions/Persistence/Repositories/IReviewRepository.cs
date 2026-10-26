using NovaLive.Domain.Reviews;

namespace NovaLive.Application.Abstractions.Persistence.Repositories;

public interface IReviewRepository
{
    Task<bool> ExistsByOrderItemAsync(Guid orderItemId, CancellationToken ct);
    Task<bool> TryAddAsync(Review review, CancellationToken ct);
    // Locks the review and tracks its full existing media collection for mutations.
    Task<Review?> GetForUpdateAsync(Guid id, CancellationToken ct);
    void ReplaceImages(Review review);
    Task<(IReadOnlyList<Review> Items, long Total)> GetVisiblePagedAsync(
        IReadOnlyCollection<Guid> skuIds, int? rating, bool? hasMedia, int page, int size, CancellationToken ct);
}
