using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Domain.Common;
using NovaLive.Domain.Shops;
namespace NovaLive.Infrastructure.Persistence.Repositories;
public sealed class ShopWalletTransactionRepository(AppDbContext db) : IShopWalletTransactionRepository
{
    public async Task AddAsync(ShopWalletTransaction transaction, CancellationToken ct) => await db.ShopWalletTransactions.AddAsync(transaction, ct);

    public Task<bool> ExistsByReferenceAsync(string refType, Guid refId, WalletTxType type, CancellationToken ct) =>
        db.ShopWalletTransactions.AnyAsync(transaction =>
            transaction.RefType == refType && transaction.RefId == refId && transaction.Type == type, ct);

    public Task<List<Guid>> GetReferenceIdsAsync(string refType, IEnumerable<Guid> refIds, WalletTxType type,
        CancellationToken ct)
    {
        var ids = refIds.Distinct().ToArray();
        return db.ShopWalletTransactions
            .Where(transaction => transaction.RefType == refType && transaction.RefId.HasValue
                && ids.Contains(transaction.RefId.Value) && transaction.Type == type)
            .Select(transaction => transaction.RefId!.Value)
            .Distinct()
            .ToListAsync(ct);
    }

    public async Task<(List<ShopWalletTransaction> Items, long TotalCount)> GetPagedAsync(
        Guid walletId,
        WalletTxType? type,
        DateTimeOffset? from,
        DateTimeOffset? to,
        int page,
        int size,
        CancellationToken ct = default)
    {
        var query = db.ShopWalletTransactions
            .AsNoTracking()
            .Where(transaction => transaction.WalletId == walletId);

        if (type.HasValue)
        {
            query = query.Where(transaction => transaction.Type == type.Value);
        }

        if (from.HasValue)
        {
            var fromUtc = from.Value.ToUniversalTime();
            query = query.Where(transaction => transaction.CreatedAt >= fromUtc);
        }

        if (to.HasValue)
        {
            var toUtc = to.Value.ToUniversalTime();
            query = query.Where(transaction => transaction.CreatedAt <= toUtc);
        }

        var total = await query.LongCountAsync(ct);
        var items = await query
            .OrderByDescending(transaction => transaction.CreatedAt)
            .ThenByDescending(transaction => transaction.Id)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        return (items, total);
    }
}
