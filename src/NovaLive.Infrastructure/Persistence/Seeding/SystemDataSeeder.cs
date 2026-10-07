using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NovaLive.Domain.Common;
using NovaLive.Domain.Rbac;
using NovaLive.Domain.Users;

namespace NovaLive.Infrastructure.Persistence.Seeding;

public sealed class SystemDataSeeder(
    AppDbContext dbContext,
    ILogger<SystemDataSeeder> logger)
    : IDataSeeder
{
    public int Order => 1;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting System Data Seeding...");

        await SeedRolesAsync(cancellationToken);
        await SeedResourcesAndPermissionsAsync(cancellationToken);
        await SeedAdminUserAsync(cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("System Data Seeding completed successfully.");
    }

    private async Task SeedRolesAsync(CancellationToken cancellationToken)
    {
        var defaultRoles = new List<Role>
        {
            new() { Name = "SuperAdmin", Description = "Toàn quyền quản trị hệ sinh thái NovaLive", IsSystem = true },
            new() { Name = "Admin", Description = "Quản trị viên vận hành sàn", IsSystem = true },
            new() { Name = "Seller", Description = "Nhà bán hàng đa kênh & Livestream", IsSystem = true },
            new() { Name = "Buyer", Description = "Người mua hàng trên nền tảng", IsSystem = true }
        };

        foreach (var role in defaultRoles)
        {
            if (!await dbContext.Roles.AnyAsync(r => r.Name == role.Name, cancellationToken))
            {
                await dbContext.Roles.AddAsync(role, cancellationToken);
            }
        }
    }

    private async Task SeedResourcesAndPermissionsAsync(CancellationToken cancellationToken)
    {
        var resources = new List<(string Code, string Description)>
        {
            ("users", "Quản lý người dùng và tài khoản"),
            ("shops", "Quản lý thông tin và ví gian hàng"),
            ("products", "Quản lý danh mục, SPU và SKU sản phẩm"),
            ("orders", "Quản lý đơn hàng, trả hàng và fulfillment"),
            ("payments", "Quản lý thanh toán, ký quỹ Escrow và Payout"),
            ("livestreams", "Quản lý phiên phát sóng trực tiếp và sản phẩm ghim"),
            ("discounts", "Quản lý mã giảm giá và chiến dịch khuyến mãi"),
            ("system", "Quản trị hệ thống, RBAC và cấu hình")
        };

        var actions = Enum.GetValues<RbacAction>();

        foreach (var (code, description) in resources)
        {
            var resource = await dbContext.Resources.FirstOrDefaultAsync(r => r.Code == code, cancellationToken);
            if (resource is null)
            {
                resource = new Resource { Code = code, Description = description };
                await dbContext.Resources.AddAsync(resource, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            foreach (var action in actions)
            {
                var permCode = $"{code}:{action.ToString().ToLowerInvariant()}";
                if (!await dbContext.Permissions.AnyAsync(p => p.Code == permCode, cancellationToken))
                {
                    var permission = new Permission
                    {
                        ResourceId = resource.Id,
                        Action = action,
                        Code = permCode,
                        Description = $"Quyền {action} đối với tài nguyên {code}"
                    };
                    await dbContext.Permissions.AddAsync(permission, cancellationToken);
                }
            }
        }
    }

    private async Task SeedAdminUserAsync(CancellationToken cancellationToken)
    {
        const string adminEmail = "admin@novalive.vn";

        if (!await dbContext.Users.AnyAsync(u => u.Email == adminEmail, cancellationToken))
        {
            var adminUser = new User(
                email: adminEmail,
                passwordHash: "$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy", // BCrypt hash for Admin@123
                fullName: "NovaLive Super Admin",
                phone: "0900000000");

            adminUser.Activate();
            await dbContext.Users.AddAsync(adminUser, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            var superAdminRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "SuperAdmin", cancellationToken);
            if (superAdminRole is not null)
            {
                await dbContext.UserRoles.AddAsync(
                    new UserRole { UserId = adminUser.Id, RoleId = superAdminRole.Id },
                    cancellationToken);
            }
        }
    }
}
