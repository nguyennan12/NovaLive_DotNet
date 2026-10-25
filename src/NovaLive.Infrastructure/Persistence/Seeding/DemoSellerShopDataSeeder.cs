using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Domain.Common;
using NovaLive.Domain.Rbac;
using NovaLive.Domain.Shops;
using NovaLive.Domain.Users;

namespace NovaLive.Infrastructure.Persistence.Seeding;

public sealed class DemoSellerShopDataSeeder(
    AppDbContext dbContext,
    IPasswordHasher passwordHasher,
    IDateTimeProvider clock,
    IOptions<DemoDataOptions> options,
    ILogger<DemoSellerShopDataSeeder> logger) : IDataSeeder
{
    public int Order => 3;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        var email = settings.SellerEmail.Trim().ToLowerInvariant();
        var adminEmail = "admin@novalive.vn";
        var buyerEmail = "buyer@novalive.vn";
        if (string.IsNullOrWhiteSpace(settings.SellerPassword))
        {
            throw new InvalidOperationException("DemoData:SellerPassword is required when demo data seeding is enabled.");
        }
        var passwordHash = passwordHasher.Hash(settings.SellerPassword);

        if (!email.EndsWith("@novalive.local", StringComparison.OrdinalIgnoreCase))
        {
            // 1. Seed Admin User
            var admin = await dbContext.Users
                .SingleOrDefaultAsync(user => user.Id == DemoDataIds.AdminId || user.Email == adminEmail, cancellationToken);

            if (admin is null)
            {
                admin = new User(adminEmail, passwordHash, "NovaLive Administrator", "0900000001");
                dbContext.Entry(admin).Property(user => user.Id).CurrentValue = DemoDataIds.AdminId;
                admin.Activate();
                await dbContext.Users.AddAsync(admin, cancellationToken);
            }
            else
            {
                admin.Activate();
                admin.ChangePassword(passwordHash);
            }

            if (!await dbContext.UserRoles.AnyAsync(
                    role => role.UserId == admin.Id && role.RoleId == SystemRoleIds.Admin,
                    cancellationToken))
            {
                await dbContext.UserRoles.AddAsync(
                    new UserRole(admin.Id, SystemRoleIds.Admin, clock.UtcNow),
                    cancellationToken);
            }

            // 2. Seed Buyer User
            var buyer = await dbContext.Users
                .SingleOrDefaultAsync(user => user.Id == DemoDataIds.BuyerId || user.Email == buyerEmail, cancellationToken);

            if (buyer is null)
            {
                buyer = new User(buyerEmail, passwordHash, "Nguyễn Văn Khách", "0900000003");
                dbContext.Entry(buyer).Property(user => user.Id).CurrentValue = DemoDataIds.BuyerId;
                buyer.Activate();
                await dbContext.Users.AddAsync(buyer, cancellationToken);
            }
            else
            {
                buyer.Activate();
                buyer.ChangePassword(passwordHash);
            }

            if (!await dbContext.UserRoles.AnyAsync(
                    role => role.UserId == buyer.Id && role.RoleId == SystemRoleIds.Buyer,
                    cancellationToken))
            {
                await dbContext.UserRoles.AddAsync(
                    new UserRole(buyer.Id, SystemRoleIds.Buyer, clock.UtcNow),
                    cancellationToken);
            }
        }

        // 3. Seed Seller User
        var seller = await dbContext.Users
            .SingleOrDefaultAsync(user => user.Id == DemoDataIds.SellerId, cancellationToken);

        if (seller is null)
        {
            seller = await dbContext.Users
                .SingleOrDefaultAsync(user => user.Email == email, cancellationToken);
        }

        if (seller is null)
        {
            seller = new User(email, passwordHash, "NovaLive Demo Seller", "0900000002");
            dbContext.Entry(seller).Property(user => user.Id).CurrentValue = DemoDataIds.SellerId;
            seller.Activate();
            seller.MarkAsSeller();
            await dbContext.Users.AddAsync(seller, cancellationToken);
        }
        else
        {
            seller.Activate();
            seller.MarkAsSeller();
            seller.ChangePassword(passwordHash);
        }

        if (!await dbContext.UserRoles.AnyAsync(
                role => role.UserId == seller.Id && role.RoleId == SystemRoleIds.Seller,
                cancellationToken))
        {
            await dbContext.UserRoles.AddAsync(
                new UserRole(seller.Id, SystemRoleIds.Seller, clock.UtcNow),
                cancellationToken);
        }

        var shop = await dbContext.Shops
            .SingleOrDefaultAsync(candidate => candidate.Id == DemoDataIds.FlagshipShopId, cancellationToken);

        if (shop is null)
        {
            shop = await dbContext.Shops
                .SingleOrDefaultAsync(candidate => candidate.OwnerId == seller.Id || candidate.Slug == "novalive-flagship", cancellationToken);
        }

        if (shop is null)
        {
            shop = new Shop(seller.Id, "NovaLive Flagship Store", "novalive-flagship");
            dbContext.Entry(shop).Property(candidate => candidate.Id).CurrentValue = DemoDataIds.FlagshipShopId;
            shop.Approve();
            await dbContext.Shops.AddAsync(shop, cancellationToken);
        }
        else if (shop.Status == ShopStatus.Pending)
        {
            shop.Approve();
        }

        var hasPrimaryWarehouse = await dbContext.ShopAddresses
            .AnyAsync(address => address.ShopId == shop.Id && address.IsPrimary, cancellationToken);
        if (!hasPrimaryWarehouse)
        {
            var warehouse = await dbContext.ShopAddresses
                .Where(address => address.ShopId == shop.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (warehouse is null)
            {
                warehouse = new ShopAddress(
                    shop.Id,
                    "Kho demo NovaLive",
                    "NovaLive Demo Seller",
                    "0900000000",
                    202,
                    "Hồ Chí Minh",
                    3695,
                    "Thành Phố Thủ Đức",
                    "90768",
                    "Phường An Khánh",
                    "123 Đường Kiểm Thử");
                await dbContext.ShopAddresses.AddAsync(warehouse, cancellationToken);
            }

            warehouse.SetPrimary(true);
            if (!await dbContext.ShopAddresses
                    .AnyAsync(address => address.ShopId == shop.Id && address.IsReturn, cancellationToken))
            {
                warehouse.SetReturn(true);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Demo accounts ready: Admin ({AdminEmail}), Buyer ({BuyerEmail}), Seller ({SellerEmail}), and Shop {ShopId}.",
            adminEmail,
            buyerEmail,
            email,
            shop.Id);
    }
}
