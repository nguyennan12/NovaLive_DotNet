using NovaLive.Domain.Common;

namespace NovaLive.Application.Abstractions.Persistence;

// Provider-specific transaction locking and unique-race recovery; ordinary reads/writes use IAppDbContext.
public interface IShopConcurrencyService
{
    // Transaction-scoped lock; acquire before loading mutable shop/address state.
    Task LockOwnerAsync(Guid ownerId, CancellationToken ct);
    Task<Error?> SaveRegistrationAsync(CancellationToken ct);
    Task SaveFollowAsync(CancellationToken ct);
}
