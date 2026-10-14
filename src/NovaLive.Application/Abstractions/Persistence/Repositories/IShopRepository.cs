using NovaLive.Domain.Common;
using NovaLive.Domain.Shops;
using NovaLive.Domain.System;

namespace NovaLive.Application.Abstractions.Persistence.Repositories;

// Shop persistence operations, including provider-specific locking and unique-race recovery.
public interface IShopRepository
{
    Task<List<Shop>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    Task<Shop?> GetByOwnerIdAsync(Guid ownerId, CancellationToken ct = default);
    Task<Shop?> GetActiveByIdAsync(Guid shopId, CancellationToken ct = default);
    // Uniqueness checks include soft-deleted shops, matching the database indexes.
    Task<bool> ExistsByOwnerAsync(Guid ownerId, CancellationToken ct = default);
    Task<bool> ExistsBySlugAsync(string slug, CancellationToken ct = default);
    Task AddAsync(Shop shop, CancellationToken ct = default);
    Task AddVerificationAsync(ShopVerification verification, CancellationToken ct = default);
    Task AddRegistrationOutboxAsync(OutboxMessage message, CancellationToken ct = default);
    Task<ShopVerification?> GetLatestVerificationAsync(Guid shopId, CancellationToken ct = default);

    // Mutation queries return tracked entities; display queries are read-only.
    Task<List<ShopAddress>> GetAddressesAsync(Guid shopId, CancellationToken ct = default);
    Task<List<ShopAddress>> GetAddressesForDeletionAsync(Guid shopId, CancellationToken ct = default);
    Task<List<ShopAddress>> GetSellerAddressesAsync(Guid shopId, CancellationToken ct = default);
    Task<List<ShopAddress>> GetPublicAddressesAsync(Guid shopId, CancellationToken ct = default);
    Task AddAddressAsync(ShopAddress address, CancellationToken ct = default);
    void DeleteAddress(ShopAddress address);

    Task<int> CountFollowersAsync(Guid shopId, CancellationToken ct = default);
    Task<bool> IsFollowingAsync(Guid shopId, Guid? userId, CancellationToken ct = default);
    Task AddFollowerAsync(ShopFollower follower, CancellationToken ct = default);
    Task RemoveFollowerAsync(Guid shopId, Guid userId, CancellationToken ct = default);

    // Transaction-scoped lock; acquire before loading mutable shop/address state.
    Task LockOwnerAsync(Guid ownerId, CancellationToken ct = default);
    Task<Error?> SaveRegistrationAsync(CancellationToken ct = default);
    Task SaveFollowAsync(CancellationToken ct = default);
}
