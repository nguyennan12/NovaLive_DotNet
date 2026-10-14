using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Domain.Carts;
using Npgsql;

namespace NovaLive.Infrastructure.Persistence.Repositories;

public sealed class CartRepository(AppDbContext dbContext, IUnitOfWork unitOfWork) : ICartRepository
{
    public Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        dbContext.Carts.SingleOrDefaultAsync(c => c.UserId == userId, ct);

    public Task<List<CartItem>> GetItemsAsync(Guid cartId, CancellationToken ct = default) =>
        dbContext.CartItems.Where(i => i.CartId == cartId).OrderBy(i => i.AddedAt).ThenBy(i => i.Id).ToListAsync(ct);

    public async Task AddAsync(Cart cart, CancellationToken ct = default) => await dbContext.Carts.AddAsync(cart, ct);
    public async Task AddItemAsync(CartItem item, CancellationToken ct = default) => await dbContext.CartItems.AddAsync(item, ct);
    public void RemoveItems(IEnumerable<CartItem> items) => dbContext.CartItems.RemoveRange(items);

    public async Task LockUserAsync(Guid userId, CancellationToken ct = default)
    {
        // All cart mutations acquire this before reading. Also serializes first-cart creation.
        if (dbContext.Database.IsNpgsql())
            await dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT pg_advisory_xact_lock(hashtextextended({userId.ToString()}, 819));", ct);
    }

    public async Task<bool> TrySaveAsync(Guid userId, CancellationToken ct = default)
    {
        try
        {
            await unitOfWork.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
            { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: "ix_carts_user_id" or "ix_cart_items_cart_id_sku_id" })
        {
            // EF rolls back its automatic savepoint, keeping the command transaction usable.
            // Remove only this aggregate's state; never clear tracked catalog/other modules.
            var carts = dbContext.ChangeTracker.Entries<Cart>().Where(e => e.Entity.UserId == userId).ToArray();
            var ids = carts.Select(e => e.Entity.Id).ToHashSet();
            foreach (var item in dbContext.ChangeTracker.Entries<CartItem>().Where(e => ids.Contains(e.Entity.CartId)).ToArray())
                item.State = EntityState.Detached;
            foreach (var cart in carts) cart.State = EntityState.Detached;
            return false;
        }
    }

    public async Task<List<CartItem>> GetCartItemsByIdsAsync(
        IEnumerable<Guid> cartItemIds,
        Guid? userId,
        CancellationToken ct = default)
    {
        var idList = cartItemIds.ToList();

        var query = from ci in dbContext.CartItems.AsNoTracking()
                    join c in dbContext.Carts.AsNoTracking() on ci.CartId equals c.Id
                    where idList.Contains(ci.Id)
                    select new { ci, c.UserId };

        if (userId.HasValue)
        {
            query = query.Where(x => x.UserId == userId.Value);
        }

        return await query.Select(x => x.ci).ToListAsync(ct);
    }
}
