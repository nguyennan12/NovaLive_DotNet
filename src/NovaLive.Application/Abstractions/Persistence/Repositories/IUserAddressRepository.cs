using NovaLive.Domain.Users;

namespace NovaLive.Application.Abstractions.Persistence.Repositories;

public interface IUserAddressRepository
{
    Task<UserAddress?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken ct = default);
}
