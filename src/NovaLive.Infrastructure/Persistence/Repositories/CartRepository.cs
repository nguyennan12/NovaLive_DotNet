using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Domain.Carts;

namespace NovaLive.Infrastructure.Persistence.Repositories;

public sealed class CartRepository(IAppDbContext dbContext) : ICartRepository
{
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
