using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NovaLive.Infrastructure.Auth;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Domain.Common;
using NovaLive.Domain.Rbac;
using NovaLive.Infrastructure.Persistence;
using NovaLive.Infrastructure.Persistence.Seeding;

namespace NovaLive.Infrastructure.Tests.Persistence;

public sealed class SystemDataSeederTests
{
    private static readonly DateTimeOffset SeedTime =
        new(2026, 10, 8, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task SeedAsync_WhenRunTwice_SeedsMatrixWithoutDuplicates()
    {
        await using var database = await SeederDatabase.CreateAsync();

        await database.SeedAsync();
        await database.SeedAsync();

        await using var context = database.CreateContext();

        (await context.Roles.CountAsync()).Should().Be(3);
        (await context.Resources.CountAsync()).Should().Be(19);
        (await context.Permissions.CountAsync()).Should().Be(58);
        (await context.Categories.CountAsync()).Should().Be(5);
        (await context.Roles.Select(role => role.Id).Distinct().CountAsync()).Should().Be(3);
        (await context.Resources.Select(resource => resource.Code).Distinct().CountAsync()).Should().Be(19);
        (await context.Permissions.Select(permission => permission.Code).Distinct().CountAsync()).Should().Be(58);
        (await context.Users.CountAsync()).Should().Be(1);
        (await context.Shops.CountAsync()).Should().Be(1);
        (await context.ShopAddresses.CountAsync()).Should().Be(1);
        (await context.Spus.CountAsync()).Should().Be(7);
        (await context.Skus.CountAsync()).Should().Be(10);
        (await context.Inventories.CountAsync()).Should().Be(10);

        var demoSeller = await context.Users.SingleAsync();
        demoSeller.Email.Should().Be("seller@novalive.local");
        demoSeller.IsSeller.Should().BeTrue();
        (await context.UserRoles.CountAsync(role =>
            role.UserId == demoSeller.Id && role.RoleId == SystemRoleIds.Seller)).Should().Be(1);

        var demoShop = await context.Shops.SingleAsync();
        demoShop.OwnerId.Should().Be(demoSeller.Id);
        demoShop.Status.Should().Be(ShopStatus.Active);

        var demoWarehouse = await context.ShopAddresses.SingleAsync();
        demoWarehouse.ShopId.Should().Be(demoShop.Id);
        demoWarehouse.IsPrimary.Should().BeTrue();
        demoWarehouse.IsReturn.Should().BeTrue();

        var roles = await context.Roles.ToDictionaryAsync(role => role.Name);
        roles.Keys.Should().BeEquivalentTo("Admin", "Seller", "Buyer");
        roles.Values.Should().OnlyContain(role => role.IsSystem);
        roles["Admin"].Id.Should().Be(SystemRoleIds.Admin);
        roles["Seller"].Id.Should().Be(SystemRoleIds.Seller);
        roles["Buyer"].Id.Should().Be(SystemRoleIds.Buyer);

        var permissionCounts = await context.RolePermissions
            .GroupBy(grant => grant.RoleId)
            .Select(group => new { RoleId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(group => group.RoleId, group => group.Count);

        permissionCounts[SystemRoleIds.Buyer].Should().Be(23);
        permissionCounts[SystemRoleIds.Seller].Should().Be(38);
        permissionCounts[SystemRoleIds.Admin].Should().Be(26);

        var registerPermissionId = await context.Permissions
            .Where(permission => permission.Code == PermissionCodes.Auth.Register)
            .Select(permission => permission.Id)
            .SingleAsync();

        (await context.RolePermissions.AnyAsync(grant => grant.PermissionId == registerPermissionId))
            .Should()
            .BeFalse();
    }

    [Fact]
    public async Task SeedAsync_WhenGrantWasRemovedAndCustomGrantWasAdded_RestoresOnlyMissingMatrixGrant()
    {
        await using var database = await SeederDatabase.CreateAsync();
        await database.SeedAsync();

        await using (var context = database.CreateContext())
        {
            var requiredPermissionId = await context.Permissions
                .Where(permission => permission.Code == PermissionCodes.Products.CreateOwn)
                .Select(permission => permission.Id)
                .SingleAsync();
            var customPermissionId = await context.Permissions
                .Where(permission => permission.Code == PermissionCodes.Returns.RequestOwn)
                .Select(permission => permission.Id)
                .SingleAsync();

            var requiredGrant = await context.RolePermissions.SingleAsync(
                grant => grant.RoleId == SystemRoleIds.Seller
                    && grant.PermissionId == requiredPermissionId);

            context.RolePermissions.Remove(requiredGrant);
            await context.RolePermissions.AddAsync(
                new RolePermission(SystemRoleIds.Seller, customPermissionId, SeedTime.AddMinutes(1)));
            await context.SaveChangesAsync();
        }

        await database.SeedAsync();

        await using var verificationContext = database.CreateContext();
        var permissionIds = await verificationContext.Permissions
            .Where(permission => permission.Code == PermissionCodes.Products.CreateOwn
                || permission.Code == PermissionCodes.Returns.RequestOwn)
            .ToDictionaryAsync(permission => permission.Code, permission => permission.Id);

        (await verificationContext.RolePermissions.AnyAsync(
                grant => grant.RoleId == SystemRoleIds.Seller
                    && grant.PermissionId == permissionIds[PermissionCodes.Products.CreateOwn]))
            .Should()
            .BeTrue();

        (await verificationContext.RolePermissions.AnyAsync(
                grant => grant.RoleId == SystemRoleIds.Seller
                    && grant.PermissionId == permissionIds[PermissionCodes.Returns.RequestOwn]))
            .Should()
            .BeTrue();
    }

    private sealed class SeederDatabase : IAsyncDisposable
    {
        private readonly SqliteConnection connection;
        private readonly DbContextOptions<AppDbContext> options;

        private SeederDatabase(SqliteConnection connection)
        {
            this.connection = connection;
            options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .UseSnakeCaseNamingConvention()
                .Options;
        }

        public static async Task<SeederDatabase> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            var database = new SeederDatabase(connection);
            await using var context = database.CreateContext();
            await context.Database.EnsureCreatedAsync();

            return database;
        }

        public AppDbContext CreateContext() => new(options);

        public async Task SeedAsync()
        {
            await using var context = CreateContext();
            var rbacSeeder = new RbacDataSeeder(
                context,
                new FixedDateTimeProvider(SeedTime),
                NullLogger<RbacDataSeeder>.Instance);
            var categorySeeder = new CategoryDataSeeder(
                context,
                NullLogger<CategoryDataSeeder>.Instance);
            var demoSellerShopSeeder = new DemoSellerShopDataSeeder(
                context,
                new PasswordHasher(),
                new FixedDateTimeProvider(SeedTime),
                Options.Create(new DemoDataOptions
                {
                    SellerEmail = "seller@novalive.local",
                    SellerPassword = "Test-only-password-123!"
                }),
                NullLogger<DemoSellerShopDataSeeder>.Instance);
            var productSeeder = new ProductDataSeeder(
                context,
                NullLogger<ProductDataSeeder>.Instance);

            await rbacSeeder.SeedAsync();
            await categorySeeder.SeedAsync();
            await demoSellerShopSeeder.SeedAsync();
            await productSeeder.SeedAsync();
        }

        public async ValueTask DisposeAsync()
        {
            await connection.DisposeAsync();
        }
    }

    private sealed class FixedDateTimeProvider(DateTimeOffset utcNow) : IDateTimeProvider
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }
}
