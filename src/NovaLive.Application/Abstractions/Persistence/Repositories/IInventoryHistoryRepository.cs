using NovaLive.Domain.Inventory;
namespace NovaLive.Application.Abstractions.Persistence.Repositories;
public interface IInventoryHistoryRepository
{
    Task AddAsync(InventoryHistory history, CancellationToken ct);
}
