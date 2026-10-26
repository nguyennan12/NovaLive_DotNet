using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Domain.Inventory;
using NovaLive.Domain.Common;
using Microsoft.EntityFrameworkCore;
namespace NovaLive.Infrastructure.Persistence.Repositories;
public sealed class InventoryHistoryRepository(AppDbContext db) : IInventoryHistoryRepository
{
    public Task<bool> ExistsByReferenceAsync(string refType, Guid refId, Guid skuId,
        InventoryChangeType changeType, CancellationToken ct) =>
        db.InventoryHistories.AnyAsync(h => h.RefType == refType && h.RefId == refId
            && h.SkuId == skuId && h.ChangeType == changeType, ct);
    public async Task AddAsync(InventoryHistory history, CancellationToken ct) => await db.InventoryHistories.AddAsync(history, ct);
}
