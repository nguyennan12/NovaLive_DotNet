using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Persistence.Repositories;

namespace NovaLive.Infrastructure.Persistence.Repositories;

public sealed class PermissionRepository(AppDbContext db) : IPermissionRepository
{
    public async Task<string[]> GetPermissionCodesAsync(
        Guid userId, IReadOnlyCollection<string> roles, CancellationToken cancellationToken = default)
    {
        if (roles.Count == 0) return [];

        return await (from ur in db.UserRoles
                      where ur.UserId == userId
                      join r in db.Roles on ur.RoleId equals r.Id
                      where roles.Contains(r.Name)
                      join rp in db.RolePermissions on r.Id equals rp.RoleId
                      join p in db.Permissions on rp.PermissionId equals p.Id
                      select p.Code)
                      .Distinct()
                      .ToArrayAsync(cancellationToken);
    }
}
