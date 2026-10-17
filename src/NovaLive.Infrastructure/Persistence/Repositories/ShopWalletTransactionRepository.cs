using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Domain.Shops;
namespace NovaLive.Infrastructure.Persistence.Repositories;
public sealed class ShopWalletTransactionRepository(AppDbContext db) : IShopWalletTransactionRepository
{
    public async Task AddAsync(ShopWalletTransaction transaction, CancellationToken ct) => await db.ShopWalletTransactions.AddAsync(transaction, ct);
}
