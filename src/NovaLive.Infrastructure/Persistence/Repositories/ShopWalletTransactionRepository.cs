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
}
