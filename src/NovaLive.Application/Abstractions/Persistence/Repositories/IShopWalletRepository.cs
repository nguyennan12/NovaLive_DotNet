using NovaLive.Domain.Shops;

namespace NovaLive.Application.Abstractions.Persistence.Repositories;

/// <summary>
/// Repository quản lý truy vấn và cập nhật ví Shop (ShopWallet) và sổ cái giao dịch (ShopWalletTransaction).
/// </summary>
public interface IShopWalletRepository
{
    Task<ShopWallet?> GetByShopIdAsync(Guid shopId, CancellationToken ct = default);

    Task AddAsync(ShopWallet wallet, CancellationToken ct = default);

    Task AddTransactionAsync(ShopWalletTransaction transaction, CancellationToken ct = default);
}
