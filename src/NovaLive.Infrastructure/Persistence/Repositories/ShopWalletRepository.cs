using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Domain.Shops;

namespace NovaLive.Infrastructure.Persistence.Repositories;

public sealed class ShopWalletRepository(AppDbContext dbContext) : IShopWalletRepository
{
    /// <inheritdoc />
    public async Task<ShopWallet?> GetByShopIdAsync(Guid shopId, CancellationToken ct = default)
    {
        return await dbContext.ShopWallets
            .FirstOrDefaultAsync(w => w.ShopId == shopId, ct);
    }

    /// <inheritdoc />
    public async Task<ShopWallet?> GetByShopIdForUpdateAsync(Guid shopId, CancellationToken ct = default)
    {
        if (dbContext.Database.IsNpgsql())
        {
            return (await dbContext.ShopWallets.FromSqlInterpolated(
                $"SELECT * FROM public.shop_wallets WHERE shop_id = {shopId} FOR UPDATE").ToListAsync(ct)).SingleOrDefault();
        }

        return await dbContext.ShopWallets.FirstOrDefaultAsync(w => w.ShopId == shopId, ct);
    }

    /// <inheritdoc />
    public async Task AddAsync(ShopWallet wallet, CancellationToken ct = default)
    {
        await dbContext.ShopWallets.AddAsync(wallet, ct);
    }

    /// <inheritdoc />
    public async Task AddTransactionAsync(ShopWalletTransaction transaction, CancellationToken ct = default)
    {
        await dbContext.ShopWalletTransactions.AddAsync(transaction, ct);
    }
}
