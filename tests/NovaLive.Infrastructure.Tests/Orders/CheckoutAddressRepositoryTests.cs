using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NovaLive.Domain.Shops;
using NovaLive.Domain.Users;
using NovaLive.Infrastructure.Persistence;
using NovaLive.Infrastructure.Persistence.Repositories;
using NovaLive.Infrastructure.Tests.Auth;

namespace NovaLive.Infrastructure.Tests.Orders;

public sealed class CheckoutAddressRepositoryTests
{
    [Fact]
    public async Task AddressQueries_EnforceOwnershipAndPrimaryWarehouse_WithoutTracking()
    {
        using var connection = new SqliteConnection("Data Source=:memory:"); await connection.OpenAsync();
        await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection)
            .ReplaceService<IModelCustomizer, SqliteTimeModelCustomizer>().UseSnakeCaseNamingConvention().Options);
        await db.Database.EnsureCreatedAsync();
        var userId = Guid.NewGuid(); var shopId = Guid.NewGuid();
        var address = new UserAddress(userId, "Buyer", "0", 1, "P", 2, "D", "W", "W", "Street");
        var foreign = new UserAddress(Guid.NewGuid(), "Other", "0", 1, "P", 2, "D", "W", "W", "Street");
        var primary = new ShopAddress(shopId, "Primary", "Seller", "0", 1, "P", 2, "D", "W", "W", "Street"); primary.SetPrimary(true);
        var secondary = new ShopAddress(shopId, "Other", "Seller", "0", 1, "P", 3, "D", "W", "W", "Street");
        var other = new ShopAddress(Guid.NewGuid(), "Primary", "Seller", "0", 1, "P", 4, "D", "W", "W", "Street"); other.SetPrimary(true);
        db.UserAddresses.AddRange(address, foreign); db.ShopAddresses.AddRange(primary, secondary, other);
        await db.SaveChangesAsync(); db.ChangeTracker.Clear();
        var addresses = new UserAddressRepository(db); var warehouses = new ShopAddressRepository(db);
        (await addresses.GetByIdForUserAsync(address.Id, userId))!.Id.Should().Be(address.Id);
        (await addresses.GetByIdForUserAsync(foreign.Id, userId)).Should().BeNull();
        (await addresses.GetByIdForUserAsync(Guid.NewGuid(), userId)).Should().BeNull();
        (await warehouses.GetPrimaryByShopIdsAsync([shopId, shopId])).Should().ContainSingle().Which.Id.Should().Be(primary.Id);
        (await warehouses.GetPrimaryByShopIdsAsync([])).Should().BeEmpty();
        db.ChangeTracker.Entries().Should().BeEmpty();
        using var source = new CancellationTokenSource(); source.Cancel();
        var cancel = () => addresses.GetByIdForUserAsync(address.Id, userId, source.Token);
        await cancel.Should().ThrowAsync<OperationCanceledException>();
    }
}
