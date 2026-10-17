using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Domain.Inventory;
namespace NovaLive.Infrastructure.Persistence.Repositories;
public sealed class InventoryHistoryRepository(AppDbContext db) : IInventoryHistoryRepository
{
    public async Task AddAsync(InventoryHistory history, CancellationToken ct) => await db.InventoryHistories.AddAsync(history, ct);
}
