using NovaLive.Application.Abstractions.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NovaLive.Application.UseCases.Shops;
using NovaLive.Domain.Common;
using NovaLive.Domain.Shops;
using NovaLive.Domain.System;

namespace NovaLive.Infrastructure.Persistence.Repositories;

public sealed class ShopRepository(AppDbContext db) : IShopRepository
{
    public Task<Shop?> GetByOwnerIdAsync(Guid ownerId, CancellationToken ct = default) =>
        db.Shops.SingleOrDefaultAsync(s => s.OwnerId == ownerId && s.DeletedAt == null, ct);

    public Task<Shop?> GetActiveByIdAsync(Guid shopId, CancellationToken ct = default) =>
        db.Shops.AsNoTracking().SingleOrDefaultAsync(s => s.Id == shopId
            && s.Status == ShopStatus.Active && s.DeletedAt == null, ct);

    public Task<bool> ExistsByOwnerAsync(Guid ownerId, CancellationToken ct = default) =>
        db.Shops.IgnoreQueryFilters().AnyAsync(s => s.OwnerId == ownerId, ct);

    public Task<bool> ExistsBySlugAsync(string slug, CancellationToken ct = default) =>
        db.Shops.IgnoreQueryFilters().AnyAsync(s => s.Slug == slug, ct);

    public async Task AddAsync(Shop shop, CancellationToken ct = default) => await db.Shops.AddAsync(shop, ct);
    public async Task AddVerificationAsync(ShopVerification verification, CancellationToken ct = default) =>
        await db.ShopVerifications.AddAsync(verification, ct);
    public async Task AddRegistrationOutboxAsync(OutboxMessage message, CancellationToken ct = default) =>
        await db.OutboxMessages.AddAsync(message, ct);

    public Task<ShopVerification?> GetLatestVerificationAsync(Guid shopId, CancellationToken ct = default) =>
        db.ShopVerifications.AsNoTracking().Where(v => v.ShopId == shopId)
            .OrderByDescending(v => v.SubmittedAt).ThenByDescending(v => v.Id).FirstOrDefaultAsync(ct);

    public Task<List<ShopAddress>> GetAddressesAsync(Guid shopId, CancellationToken ct = default) =>
        db.ShopAddresses.Where(a => a.ShopId == shopId).ToListAsync(ct);

    public Task<List<ShopAddress>> GetAddressesForDeletionAsync(Guid shopId, CancellationToken ct = default) =>
        db.ShopAddresses.Where(a => a.ShopId == shopId)
            .OrderBy(a => a.CreatedAt).ThenBy(a => a.Id).ToListAsync(ct);

    public Task<List<ShopAddress>> GetSellerAddressesAsync(Guid shopId, CancellationToken ct = default) =>
        db.ShopAddresses.AsNoTracking().Where(a => a.ShopId == shopId)
            .OrderByDescending(a => a.IsPrimary).ThenBy(a => a.CreatedAt).ThenBy(a => a.Id).ToListAsync(ct);

    public Task<List<ShopAddress>> GetPublicAddressesAsync(Guid shopId, CancellationToken ct = default) =>
        db.ShopAddresses.AsNoTracking().Where(a => a.ShopId == shopId)
            .OrderByDescending(a => a.IsPrimary).ThenBy(a => a.Id).ToListAsync(ct);

    public async Task AddAddressAsync(ShopAddress address, CancellationToken ct = default) =>
        await db.ShopAddresses.AddAsync(address, ct);
    public void DeleteAddress(ShopAddress address) => db.ShopAddresses.Remove(address);

    public Task<int> CountFollowersAsync(Guid shopId, CancellationToken ct = default) =>
        db.ShopFollowers.CountAsync(f => f.ShopId == shopId, ct);
    public Task<bool> IsFollowingAsync(Guid shopId, Guid? userId, CancellationToken ct = default) =>
        db.ShopFollowers.AnyAsync(f => f.ShopId == shopId && f.UserId == userId, ct);
    public async Task AddFollowerAsync(ShopFollower follower, CancellationToken ct = default) =>
        await db.ShopFollowers.AddAsync(follower, ct);
    public async Task RemoveFollowerAsync(Guid shopId, Guid userId, CancellationToken ct = default) =>
        await db.ShopFollowers.Where(f => f.ShopId == shopId && f.UserId == userId).ExecuteDeleteAsync(ct);

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
