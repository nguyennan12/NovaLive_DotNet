using NovaLive.Domain.Users;

namespace NovaLive.Application.Abstractions.Persistence.Repositories;

public interface IUserRepository
{
    Task<List<User>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    Task<User?> GetTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
