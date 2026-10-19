using NovaLive.Domain.Common;
using NovaLive.Domain.Shops;
namespace NovaLive.Application.Abstractions.Persistence.Repositories;
public interface IShopWalletTransactionRepository
{
    Task AddAsync(ShopWalletTransaction transaction, CancellationToken ct);

    Task<bool> ExistsByReferenceAsync(string refType, Guid refId, WalletTxType type, CancellationToken ct);

    Task<List<Guid>> GetReferenceIdsAsync(string refType, IEnumerable<Guid> refIds, WalletTxType type,
        CancellationToken ct);
}
