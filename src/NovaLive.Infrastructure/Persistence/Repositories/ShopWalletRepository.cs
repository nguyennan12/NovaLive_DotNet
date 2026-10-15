using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Domain.Shops;

namespace NovaLive.Infrastructure.Persistence.Repositories;

/// <summary>
/// Triển khai repository quản lý ví Shop sử dụng AppDbContext.
/// </summary>
public sealed class ShopWalletRepository(AppDbContext dbContext) : IShopWalletRepository
{
    public async Task<ShopWallet?> GetByShopIdAsync(Guid shopId, CancellationToken ct = default)
    {
        return await dbContext.ShopWallets
            .FirstOrDefaultAsync(w => w.ShopId == shopId, ct);
    }

    public async Task AddAsync(ShopWallet wallet, CancellationToken ct = default)
    {
        await dbContext.ShopWallets.AddAsync(wallet, ct);
    }

    public async Task AddTransactionAsync(ShopWalletTransaction transaction, CancellationToken ct = default)
    {
        await dbContext.ShopWalletTransactions.AddAsync(transaction, ct);
    }
}
