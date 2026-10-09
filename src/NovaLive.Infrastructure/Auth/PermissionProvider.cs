using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Cache;
using NovaLive.Infrastructure.Persistence;
namespace NovaLive.Infrastructure.Auth;

public sealed class PermissionProvider(AppDbContext db, ICacheService cache) : IPermissionProvider
{
    public async Task<IReadOnlyCollection<string>> GetPermissionsAsync(
        IReadOnlyCollection<string> roles, CancellationToken cancellationToken = default)
    {
        var roleIds = await db.Roles.Where(role => roles.Contains(role.Name))
            .Select(role => role.Id).ToListAsync(cancellationToken);
        var permissions = new HashSet<string>(StringComparer.Ordinal);
        foreach (var roleId in roleIds)
        {
            var codes = await cache.GetOrSetAsync($"rbac:role-perms:{roleId}", async ct =>
                await (from grant in db.RolePermissions
                       join permission in db.Permissions on grant.PermissionId equals permission.Id
                       where grant.RoleId == roleId select permission.Code).ToArrayAsync(ct),
                TimeSpan.FromMinutes(5), cancellationToken);
            permissions.UnionWith(codes);
        }
        return permissions;
    }
    public Task InvalidateAsync(Guid roleId, CancellationToken cancellationToken = default) =>
        cache.RemoveAsync($"rbac:role-perms:{roleId}", cancellationToken);
}

