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

        var seller = await dbContext.Users
            .SingleOrDefaultAsync(user => user.Id == DemoDataIds.SellerId, cancellationToken);

        if (seller is null)
        {
            seller = await dbContext.Users
                .SingleOrDefaultAsync(user => user.Email == email, cancellationToken);
        }

        if (seller is null)
        {
            if (string.IsNullOrWhiteSpace(settings.SellerPassword))
            {
                throw new InvalidOperationException(
                    "DemoData:SellerPassword is required when demo seeding must create a new seller account.");
            }

            seller = new User(email, passwordHasher.Hash(settings.SellerPassword), "NovaLive Demo Seller");
            dbContext.Entry(seller).Property(user => user.Id).CurrentValue = DemoDataIds.SellerId;
            seller.Activate();
            seller.MarkAsSeller();
            await dbContext.Users.AddAsync(seller, cancellationToken);
        }
        else
        {
            seller.Activate();
            seller.MarkAsSeller();
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
                .OrderBy(address => address.CreatedAt)
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
            "Demo seller {SellerEmail} and shop {ShopId} are ready.",
            email,
            shop.Id);
    }
}
