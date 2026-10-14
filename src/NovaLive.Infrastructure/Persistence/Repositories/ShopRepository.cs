using NovaLive.Application.Abstractions.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.UseCases.Shops;
using NovaLive.Domain.Common;
using NovaLive.Domain.Shops;

namespace NovaLive.Infrastructure.Persistence.Repositories;

public sealed class ShopRepository(AppDbContext db) : IShopRepository
{
    public async Task LockOwnerAsync(Guid ownerId, CancellationToken ct = default)
    {
        if (db.Database.IsNpgsql())
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT pg_advisory_xact_lock(hashtextextended({ownerId.ToString()}, 811));", ct);
    }

    public async Task<Error?> SaveRegistrationAsync(CancellationToken ct = default)
    {
        var ownerId = db.ChangeTracker.Entries<Shop>()
            .SingleOrDefault(entry => entry.State == EntityState.Added)?.Entity.OwnerId;
        try
        {
            await db.SaveChangesAsync(ct);
            return null;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
            { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: "ix_shops_owner_id" or "ix_shops_slug" })
        {
            // TransactionBehavior rolls the entire registration back on this failed Result.
            db.ChangeTracker.Clear();
            // Both keys can collide for simultaneous identical registrations. PostgreSQL may
            // report either index; after EF's savepoint rollback, prioritize the owner invariant.
            if (((PostgresException)ex.InnerException).ConstraintName == "ix_shops_owner_id"
                || (ownerId.HasValue && await db.Shops.IgnoreQueryFilters().AnyAsync(s => s.OwnerId == ownerId, ct)))
                return ShopErrors.AlreadyOwned;
            return ShopErrors.SlugTaken;
        }
    }

    public async Task SaveFollowAsync(CancellationToken ct = default)
    {
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
            { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: "ix_shop_followers_shop_id_user_id" })
        {
            // EF rolls back to its automatic savepoint inside the command transaction.
            // Detach the losing insert so the transaction's final save does not retry it.
            foreach (var entry in db.ChangeTracker.Entries<ShopFollower>().Where(e => e.State == EntityState.Added).ToArray())
                entry.State = EntityState.Detached;
        }
    }
}
