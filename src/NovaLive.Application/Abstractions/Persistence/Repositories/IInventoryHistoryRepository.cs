using NovaLive.Domain.Inventory;
namespace NovaLive.Application.Abstractions.Persistence.Repositories;
public interface IInventoryHistoryRepository
{
    Task<bool> ExistsByReferenceAsync(string refType, Guid refId, Guid skuId,
        NovaLive.Domain.Common.InventoryChangeType changeType, CancellationToken ct);
    Task AddAsync(InventoryHistory history, CancellationToken ct);
}
