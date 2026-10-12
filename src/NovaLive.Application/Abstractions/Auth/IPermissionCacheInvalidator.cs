namespace NovaLive.Application.Abstractions.Auth;

public interface IPermissionCacheInvalidator
{
    Task InvalidateRoleAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task InvalidateAllAsync(CancellationToken cancellationToken = default);
}
