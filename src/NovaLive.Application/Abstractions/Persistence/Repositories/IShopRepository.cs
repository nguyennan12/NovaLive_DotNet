using NovaLive.Domain.Common;

namespace NovaLive.Application.Abstractions.Persistence.Repositories;

// Shop persistence operations, including provider-specific locking and unique-race recovery.
public interface IShopRepository
{
    // Transaction-scoped lock; acquire before loading mutable shop/address state.
    Task LockOwnerAsync(Guid ownerId, CancellationToken ct = default);
    Task<Error?> SaveRegistrationAsync(CancellationToken ct = default);
    Task SaveFollowAsync(CancellationToken ct = default);
}
