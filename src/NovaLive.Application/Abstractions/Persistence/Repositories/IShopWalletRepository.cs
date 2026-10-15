using NovaLive.Domain.Shops;

namespace NovaLive.Application.Abstractions.Persistence.Repositories;

public interface IShopWalletRepository
{
    Task<ShopWallet?> GetByShopIdAsync(Guid shopId, CancellationToken ct = default);

    Task<ShopWallet?> GetByShopIdForUpdateAsync(Guid shopId, CancellationToken ct = default);

    Task AddAsync(ShopWallet wallet, CancellationToken ct = default);

    Task AddTransactionAsync(ShopWalletTransaction transaction, CancellationToken ct = default);
}
