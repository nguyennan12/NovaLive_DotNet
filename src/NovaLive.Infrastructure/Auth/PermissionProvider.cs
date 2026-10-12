using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Infrastructure.Persistence;
namespace NovaLive.Infrastructure.Auth;

public sealed class PermissionProvider(AppDbContext db, RolePermissionCache cache) : IPermissionProvider
{
    public async Task<IReadOnlyCollection<string>> GetPermissionsAsync(
        IReadOnlyCollection<string> roles, CancellationToken cancellationToken = default)
    {
        var roleIds = await db.Roles.Where(role => roles.Contains(role.Name))
            .Select(role => role.Id).ToListAsync(cancellationToken);
        var permissions = new HashSet<string>(StringComparer.Ordinal);
        foreach (var roleId in roleIds)
        {
            async Task<string[]> Load(CancellationToken ct) =>
                await (from grant in db.RolePermissions
                       join permission in db.Permissions on grant.PermissionId equals permission.Id
                       where grant.RoleId == roleId select permission.Code).Distinct().ToArrayAsync(ct);
            // Never publish data read inside an uncommitted transaction to the shared cache.
            var codes = db.Database.CurrentTransaction is not null
                ? await Load(cancellationToken)
                : await cache.GetAsync(roleId, Load, cancellationToken);
            permissions.UnionWith(codes);
        }
        return permissions;
    }
    public Task InvalidateAsync(Guid roleId, CancellationToken cancellationToken = default) =>
        cache.InvalidateRoleAsync(roleId, cancellationToken);
}

