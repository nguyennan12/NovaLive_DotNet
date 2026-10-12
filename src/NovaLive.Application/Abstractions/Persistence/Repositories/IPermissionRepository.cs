namespace NovaLive.Application.Abstractions.Persistence.Repositories;

public interface IPermissionRepository
{
    Task<string[]> GetPermissionCodesAsync(Guid userId, IReadOnlyCollection<string> roles, CancellationToken cancellationToken = default);
}
