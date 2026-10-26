using NovaLive.Domain.Orders;

namespace NovaLive.Application.Abstractions.Persistence.Repositories;

public interface IParentOrderRepository
{
    Task<ParentOrder?> FindAsync(Guid id, Guid? buyerId, CancellationToken ct);

    Task<ParentOrder?> FindByOrderCodeAsync(string orderCode, Guid? buyerId, CancellationToken ct);

    Task<ParentOrder?> GetForUpdateAsync(Guid id, CancellationToken ct);

    Task<ParentOrder?> GetByOrderCodeForUpdateAsync(string orderCode, CancellationToken ct);

    Task<(IReadOnlyList<ParentOrder> Items, long Total)> GetMinePagedAsync(Guid buyerId,
        string? status, DateTimeOffset? from, DateTimeOffset? to, int page, int size, CancellationToken ct);

    Task<bool> TryAddAsync(ParentOrder order, CancellationToken ct);
}
