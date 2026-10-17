using NovaLive.Domain.Shops;
namespace NovaLive.Application.Abstractions.Persistence.Repositories;
public interface IShopWalletTransactionRepository
{
    Task AddAsync(ShopWalletTransaction transaction, CancellationToken ct);
}
