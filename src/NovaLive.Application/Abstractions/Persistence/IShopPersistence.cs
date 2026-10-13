using NovaLive.Domain.Common;

namespace NovaLive.Application.Abstractions.Persistence;

public interface IShopPersistence
{
    // Transaction-scoped lock; acquire before loading mutable shop/address state.
    Task LockOwnerAsync(Guid ownerId, CancellationToken ct);
    Task<Error?> SaveRegistrationAsync(CancellationToken ct);
    Task SaveFollowAsync(CancellationToken ct);
    Task RemoveFollowAsync(Guid shopId, Guid userId, CancellationToken ct);
}
