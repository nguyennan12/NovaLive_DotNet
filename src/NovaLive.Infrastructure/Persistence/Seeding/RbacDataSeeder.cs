using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NovaLive.Application.Abstractions.Clock;
using NovaLive.Domain.Rbac;

namespace NovaLive.Infrastructure.Persistence.Seeding;

public sealed class RbacDataSeeder(
    AppDbContext dbContext,
    IDateTimeProvider dateTimeProvider,
    ILogger<RbacDataSeeder> logger) : IDataSeeder
{
    private const long AdvisoryLockKey = 748923748293748923L;

    public int Order => 1;

    private static readonly RoleSeed[] SystemRoles =
    [
        new(SystemRoleIds.Admin, "Admin", "Quản trị viên toàn quyền hệ thống"),
        new(SystemRoleIds.Seller, "Seller", "Chủ gian hàng bán sản phẩm và livestream"),
        new(SystemRoleIds.Buyer, "Buyer", "Khách hàng mua sắm trên nền tảng")
    ];

    private static readonly ResourceSeed[] Resources =
    [
        new("auth", "Xác thực và phân quyền"),
        new("users", "Quản lý người dùng"),
        new("shops", "Quản lý gian hàng"),
        new("wallets", "Ví tiền gian hàng"),
        new("categories", "Danh mục sản phẩm"),
        new("products", "Sản phẩm SPU/SKU"),
        new("inventory", "Kho hàng và tồn kho"),
        new("carts", "Giỏ hàng"),
        new("orders", "Đơn hàng"),
        new("payments", "Thanh toán và đối soát"),
        new("discounts", "Mã giảm giá"),
        new("flashsales", "Chương trình Flash Sale"),
        new("shipping", "Vận chuyển và giao nhận"),
        new("returns", "Trả hàng và hoàn tiền"),
        new("disputes", "Khiếu nại và tranh chấp"),
        new("livestreams", "Livestream bán hàng"),
        new("reviews", "Đánh giá sản phẩm"),
        new("reports", "Báo cáo và thống kê"),
        new("roles", "Quản trị vai trò và quyền hạn")
    ];

    private static readonly PermissionSeed[] Permissions =
    [
        new(PermissionCodes.Auth.Register, RoleGrant.None),
        new(PermissionCodes.Auth.Login, RoleGrant.None),
        new(PermissionCodes.Auth.Logout, RoleGrant.Buyer | RoleGrant.Seller | RoleGrant.Admin),
        new(PermissionCodes.Users.ViewOwnProfile, RoleGrant.Buyer | RoleGrant.Seller | RoleGrant.Admin),
        new(PermissionCodes.Users.UpdateOwnProfile, RoleGrant.Buyer | RoleGrant.Seller | RoleGrant.Admin),
        new(PermissionCodes.Users.ManageOwnAddresses, RoleGrant.Buyer | RoleGrant.Seller),
        new(PermissionCodes.Users.ManageAll, RoleGrant.Admin),
        new(PermissionCodes.Shops.Register, RoleGrant.Buyer),
        new(PermissionCodes.Shops.ViewPublic, RoleGrant.Buyer | RoleGrant.Seller | RoleGrant.Admin),
        new(PermissionCodes.Shops.ManageOwn, RoleGrant.Seller),
        new(PermissionCodes.Shops.Follow, RoleGrant.Buyer | RoleGrant.Seller),
        new(PermissionCodes.Shops.ApproveKyc, RoleGrant.Admin),
        new(PermissionCodes.Shops.BanUnban, RoleGrant.Admin),
        new(PermissionCodes.Wallets.ViewOwn, RoleGrant.Seller),
        new(PermissionCodes.Wallets.RequestPayout, RoleGrant.Seller),
        new(PermissionCodes.Wallets.ApprovePayout, RoleGrant.Admin),
        new(PermissionCodes.Categories.ViewPublic, RoleGrant.Buyer | RoleGrant.Seller | RoleGrant.Admin),
        new(PermissionCodes.Categories.ManageAll, RoleGrant.Admin),
        new(PermissionCodes.Products.ViewPublic, RoleGrant.Buyer | RoleGrant.Seller | RoleGrant.Admin),
        new(PermissionCodes.Products.ViewOwn, RoleGrant.Seller),
        new(PermissionCodes.Products.CreateOwn, RoleGrant.Seller),
        new(PermissionCodes.Products.UpdateOwn, RoleGrant.Seller),
        new(PermissionCodes.Products.DeleteOwn, RoleGrant.Seller),
        new(PermissionCodes.Products.ModerateAll, RoleGrant.Admin),
        new(PermissionCodes.Inventory.ManageOwn, RoleGrant.Seller),
        new(PermissionCodes.Carts.ManageOwn, RoleGrant.Buyer | RoleGrant.Seller),
        new(PermissionCodes.Orders.Checkout, RoleGrant.Buyer | RoleGrant.Seller),
        new(PermissionCodes.Orders.ViewOwnBuy, RoleGrant.Buyer | RoleGrant.Seller),
        new(PermissionCodes.Orders.CancelOwnBuy, RoleGrant.Buyer | RoleGrant.Seller),
        new(PermissionCodes.Orders.ViewOwnSell, RoleGrant.Seller),
        new(PermissionCodes.Orders.FulfillmentOwn, RoleGrant.Seller),
        new(PermissionCodes.Orders.CancelOwnSell, RoleGrant.Seller),
        new(PermissionCodes.Orders.ViewAllPlatform, RoleGrant.Admin),
        new(PermissionCodes.Payments.Initiate, RoleGrant.Buyer | RoleGrant.Seller),
        new(PermissionCodes.Payments.ViewStatus, RoleGrant.Buyer | RoleGrant.Seller | RoleGrant.Admin),
        new(PermissionCodes.Discounts.ViewPublic, RoleGrant.Buyer | RoleGrant.Seller | RoleGrant.Admin),
        new(PermissionCodes.Discounts.CreateOwnShop, RoleGrant.Seller),
        new(PermissionCodes.Discounts.CreatePlatform, RoleGrant.Admin),
        new(PermissionCodes.FlashSales.ViewPublic, RoleGrant.Buyer | RoleGrant.Seller | RoleGrant.Admin),
        new(PermissionCodes.FlashSales.RegisterOwn, RoleGrant.Seller),
        new(PermissionCodes.FlashSales.ManageAll, RoleGrant.Admin),
        new(PermissionCodes.Shipping.CalculateFee, RoleGrant.Buyer | RoleGrant.Seller | RoleGrant.Admin),
        new(PermissionCodes.Shipping.PrintOwnLabel, RoleGrant.Seller),
        new(PermissionCodes.Shipping.TrackOrder, RoleGrant.Buyer | RoleGrant.Seller | RoleGrant.Admin),
        new(PermissionCodes.Returns.RequestOwn, RoleGrant.Buyer),
        new(PermissionCodes.Returns.RespondOwn, RoleGrant.Seller),
        new(PermissionCodes.Disputes.ArbitrateAll, RoleGrant.Admin),
        new(PermissionCodes.Livestreams.ViewPublic, RoleGrant.Buyer | RoleGrant.Seller | RoleGrant.Admin),
        new(PermissionCodes.Livestreams.StartOwn, RoleGrant.Seller),
        new(PermissionCodes.Livestreams.PinOwnProduct, RoleGrant.Seller),
        new(PermissionCodes.Livestreams.TerminateAll, RoleGrant.Admin),
        new(PermissionCodes.Reviews.CreateOwn, RoleGrant.Buyer),
        new(PermissionCodes.Reviews.UpdateOwn, RoleGrant.Buyer),
        new(PermissionCodes.Reviews.ReplyOwnShop, RoleGrant.Seller),
        new(PermissionCodes.Reviews.HideAll, RoleGrant.Admin),
        new(PermissionCodes.Reports.ViewOwnShop, RoleGrant.Seller),
        new(PermissionCodes.Reports.ViewAllPlatform, RoleGrant.Admin),
        new(PermissionCodes.Roles.ManageAll, RoleGrant.Admin)
    ];

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting RBAC data seeding.");

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        if (dbContext.Database.ProviderName == "Npgsql.EntityFrameworkCore.PostgreSQL")
        {
            await dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT pg_advisory_xact_lock({AdvisoryLockKey});",
                cancellationToken);
        }

        await SeedRolesAsync(cancellationToken);
        await SeedResourcesAsync(cancellationToken);
        await SeedPermissionsAsync(cancellationToken);
        await SeedRolePermissionsAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        logger.LogInformation("RBAC data seeding completed successfully.");
    }

    private async Task SeedRolesAsync(CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.UtcNow;

        foreach (var role in SystemRoles)
        {
            var existingById = await dbContext.Roles
                .SingleOrDefaultAsync(existing => existing.Id == role.Id, cancellationToken);
            var existingByName = await dbContext.Roles
                .SingleOrDefaultAsync(existing => existing.Name == role.Name, cancellationToken);

            if (existingByName is not null && existingByName.Id != role.Id)
            {
                existingById = await MigrateLegacyRoleAsync(
                    role,
                    existingById,
                    existingByName,
                    now,
                    cancellationToken);
            }

            if (existingById is null)
            {
                existingById = new Role(role.Id, role.Name, now, role.Description, isSystem: true);
                await dbContext.Roles.AddAsync(existingById, cancellationToken);
            }
            else
            {
                existingById.UpdateSystemDefinition(role.Name, role.Description);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<Role> MigrateLegacyRoleAsync(
        RoleSeed role,
        Role? fixedRole,
        Role legacyRole,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Migrating system role {RoleName} from legacy ID {LegacyRoleId} to fixed ID {FixedRoleId}.",
            role.Name,
            legacyRole.Id,
            role.Id);

        var temporaryName = $"__legacy_{role.Name}_{legacyRole.Id:N}";
        await dbContext.Roles
            .Where(existing => existing.Id == legacyRole.Id)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(existing => existing.Name, temporaryName),
                cancellationToken);
        dbContext.ChangeTracker.Clear();

        fixedRole = await dbContext.Roles
            .SingleOrDefaultAsync(existing => existing.Id == role.Id, cancellationToken);

        if (fixedRole is null)
        {
            fixedRole = new Role(role.Id, role.Name, now, role.Description, isSystem: true);
            await dbContext.Roles.AddAsync(fixedRole, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var legacyGrants = await dbContext.RolePermissions
            .Where(grant => grant.RoleId == legacyRole.Id)
            .ToListAsync(cancellationToken);
        var fixedPermissionIds = await dbContext.RolePermissions
            .Where(grant => grant.RoleId == role.Id)
            .Select(grant => grant.PermissionId)
            .ToHashSetAsync(cancellationToken);

        foreach (var grant in legacyGrants.Where(grant => fixedPermissionIds.Add(grant.PermissionId)))
        {
            await dbContext.RolePermissions.AddAsync(
                new RolePermission(role.Id, grant.PermissionId, grant.GrantedAt),
                cancellationToken);
        }

        var legacyUserRoles = await dbContext.UserRoles
            .Where(userRole => userRole.RoleId == legacyRole.Id)
            .ToListAsync(cancellationToken);
        var fixedUserIds = await dbContext.UserRoles
            .Where(userRole => userRole.RoleId == role.Id)
            .Select(userRole => userRole.UserId)
            .ToHashSetAsync(cancellationToken);

        foreach (var userRole in legacyUserRoles.Where(userRole => fixedUserIds.Add(userRole.UserId)))
        {
            await dbContext.UserRoles.AddAsync(
                new UserRole(userRole.UserId, role.Id, userRole.GrantedAt),
                cancellationToken);
        }

        dbContext.RolePermissions.RemoveRange(legacyGrants);
        dbContext.UserRoles.RemoveRange(legacyUserRoles);
        await dbContext.SaveChangesAsync(cancellationToken);

        var legacyRoleToDelete = await dbContext.Roles
            .SingleAsync(existing => existing.Id == legacyRole.Id, cancellationToken);
        dbContext.Roles.Remove(legacyRoleToDelete);
        await dbContext.SaveChangesAsync(cancellationToken);

        return fixedRole;
    }

    private async Task SeedResourcesAsync(CancellationToken cancellationToken)
    {
        var resourceCodes = Resources.Select(resource => resource.Code).ToArray();
        var existingCodes = await dbContext.Resources
            .Where(resource => resourceCodes.Contains(resource.Code))
            .Select(resource => resource.Code)
            .ToHashSetAsync(cancellationToken);

        foreach (var resource in Resources.Where(resource => !existingCodes.Contains(resource.Code)))
        {
            await dbContext.Resources.AddAsync(
                new Resource(resource.Code, resource.Description),
                cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedPermissionsAsync(CancellationToken cancellationToken)
    {
        var permissionCodes = Permissions.Select(permission => permission.Code).ToArray();
        var existingPermissions = await dbContext.Permissions
            .Where(permission => permissionCodes.Contains(permission.Code))
            .ToDictionaryAsync(permission => permission.Code, cancellationToken);
        var resources = await dbContext.Resources
            .Where(resource => resourceCodesForPermissions.Contains(resource.Code))
            .ToDictionaryAsync(resource => resource.Code, cancellationToken);

        foreach (var permission in Permissions)
        {
            var parts = permission.Code.Split(':', 2);
            var resourceCode = parts[0];
            var action = parts[1];

            if (!existingPermissions.TryGetValue(permission.Code, out var existing))
            {
                var resource = resources[resourceCode];
                await dbContext.Permissions.AddAsync(
                    new Permission(resource.Id, action, permission.Code, $"{action} on {resourceCode}"),
                    cancellationToken);
            }
            else
            {
                existing.UpdateDefinition(action, $"{action} on {resourceCode}");
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static readonly string[] resourceCodesForPermissions =
        Permissions.Select(p => p.Code.Split(':', 2)[0]).Distinct().ToArray();

    private async Task SeedRolePermissionsAsync(CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.UtcNow;
        var permissionCodes = Permissions.Select(permission => permission.Code).ToArray();
        var permissionByCode = await dbContext.Permissions
            .Where(permission => permissionCodes.Contains(permission.Code))
            .ToDictionaryAsync(permission => permission.Code, cancellationToken);

        var existingGrants = (await dbContext.RolePermissions
            .Where(grant => grant.RoleId == SystemRoleIds.Admin
                || grant.RoleId == SystemRoleIds.Seller
                || grant.RoleId == SystemRoleIds.Buyer)
            .ToListAsync(cancellationToken))
            .Select(grant => (grant.RoleId, grant.PermissionId))
            .ToHashSet();

        foreach (var permission in Permissions)
        {
            var permissionId = permissionByCode[permission.Code].Id;

            await AddGrantIfMissingAsync(SystemRoleIds.Buyer, RoleGrant.Buyer, permission, permissionId);
            await AddGrantIfMissingAsync(SystemRoleIds.Seller, RoleGrant.Seller, permission, permissionId);
            await AddGrantIfMissingAsync(SystemRoleIds.Admin, RoleGrant.Admin, permission, permissionId);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return;

        async Task AddGrantIfMissingAsync(
            Guid roleId,
            RoleGrant roleGrant,
            PermissionSeed permission,
            Guid permissionId)
        {
            if (!permission.Grants.HasFlag(roleGrant)
                || !existingGrants.Add((roleId, permissionId)))
            {
                return;
            }

            await dbContext.RolePermissions.AddAsync(
                new RolePermission(roleId, permissionId, now),
                cancellationToken);
        }
    }

    private sealed record RoleSeed(Guid Id, string Name, string Description);

    private sealed record ResourceSeed(string Code, string Description);

    private sealed record PermissionSeed(string Code, RoleGrant Grants);

    [Flags]
    private enum RoleGrant
    {
        None = 0,
        Buyer = 1,
        Seller = 2,
        Admin = 4
    }
}
