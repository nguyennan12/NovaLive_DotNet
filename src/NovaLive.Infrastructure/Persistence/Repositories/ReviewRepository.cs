using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Domain.Reviews;
using Npgsql;

namespace NovaLive.Infrastructure.Persistence.Repositories;

public sealed class ReviewRepository(AppDbContext db) : IReviewRepository
{
    public Task<bool> ExistsByOrderItemAsync(Guid orderItemId, CancellationToken ct) =>
        db.Reviews.AsNoTracking().AnyAsync(review => review.OrderItemId == orderItemId, ct);

    public async Task<bool> TryAddAsync(Review review, CancellationToken ct)
    {
        db.Reviews.Add(review);
        try
        {
            await db.SaveChangesAsync(ct);
            // Flush the parent first so schemas with a review_images FK also work.
            // Media is committed by the outer command transaction; a later failure rolls back the parent too.
            db.ReviewImages.AddRange(review.Images);
            return true;
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
            { SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: "ix_reviews_order_item_id" or "reviews_order_item_id_key" })
        {
            // EF restores its transaction savepoint. The pipeline then rolls back the failed Result.
            foreach (var image in review.Images.ToArray()) db.Entry(image).State = EntityState.Detached;
            db.Entry(review).State = EntityState.Detached;
            return false;
        }
    }

    public async Task<Review?> GetForUpdateAsync(Guid id, CancellationToken ct)
    {
        if (db.Database.CurrentTransaction is null)
            throw new InvalidOperationException("Review mutations require the command transaction.");

        // Serialize edits, replies and moderation before evaluating the aggregate's current state.
        var review = db.Database.IsNpgsql()
            ? (await db.Reviews.FromSqlInterpolated(
                $"SELECT * FROM public.reviews WHERE id = {id} FOR UPDATE").ToListAsync(ct)).SingleOrDefault()
            : await db.Reviews.SingleOrDefaultAsync(review => review.Id == id, ct);
        if (review is not null)
            review.RestoreImages(await db.ReviewImages.Where(image => image.ReviewId == id).ToListAsync(ct));
        return review;
    }

    public void ReplaceImages(Review review)
    {
        // GetForUpdateAsync loads all old rows into this unit of work before the domain edit.
        db.ReviewImages.RemoveRange(db.ReviewImages.Local.Where(image => image.ReviewId == review.Id).ToArray());
        db.ReviewImages.AddRange(review.Images);
    }

    public async Task<(IReadOnlyList<Review> Items, long Total)> GetVisiblePagedAsync(
        IReadOnlyCollection<Guid> skuIds, int? rating, bool? hasMedia, int page, int size, CancellationToken ct)
    {
        var query = db.Reviews.AsNoTracking().Where(review => review.IsVisible && skuIds.Contains(review.SkuId));
        if (rating.HasValue) query = query.Where(review => review.Rating == rating.Value);
        if (hasMedia.HasValue)
            query = query.Where(review => db.ReviewImages.Any(image => image.ReviewId == review.Id) == hasMedia.Value);
        var total = await query.LongCountAsync(ct);
        var items = await query.OrderByDescending(review => review.CreatedAt).ThenByDescending(review => review.Id)
            .Skip((page - 1) * size).Take(size).ToListAsync(ct);
        var reviewIds = items.Select(review => review.Id).ToArray();
        var images = await db.ReviewImages.AsNoTracking().Where(image => reviewIds.Contains(image.ReviewId)).ToListAsync(ct);
        var byReview = images.ToLookup(image => image.ReviewId);
        foreach (var review in items) review.RestoreImages(byReview[review.Id].ToArray());
        return (items, total);
    }
}
