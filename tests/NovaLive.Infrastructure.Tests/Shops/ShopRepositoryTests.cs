using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NovaLive.Domain.Common;
using NovaLive.Domain.Products;
using NovaLive.Domain.Shops;
using NovaLive.Domain.System;
using NovaLive.Domain.Users;
using NovaLive.Infrastructure.Persistence;
using NovaLive.Infrastructure.Persistence.Repositories;
using NovaLive.Infrastructure.Tests.Auth;

namespace NovaLive.Infrastructure.Tests.Shops;

public sealed class ShopRepositoryTests
{
    private static readonly DateTimeOffset Time = new(2026, 10, 14, 0, 0, 0, TimeSpan.Zero);
    private static Guid Id(int value) => Guid.Parse($"00000000-0000-0000-0000-{value:D12}");

    [Theory]
    [InlineData(ShopStatus.Pending)]
    [InlineData(ShopStatus.Active)]
    [InlineData(ShopStatus.Suspended)]
    [InlineData(ShopStatus.Banned)]
    [InlineData(ShopStatus.Closed)]
    public async Task OwnerLookupTracksAllStatuses_PublicLookupOnlyReturnsActiveWithoutTracking(ShopStatus status)
    {
        using var fixture = new Fixture();
        var db = fixture.Db;
        var shop = new Shop(Id(1), "Shop", "shop");
        db.Entry(shop).Property(s => s.Status).CurrentValue = status;
        await fixture.Repository.AddAsync(shop);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var publicShop = await fixture.Repository.GetActiveByIdAsync(shop.Id);
        (publicShop is not null).Should().Be(status == ShopStatus.Active);
        db.ChangeTracker.Entries().Should().BeEmpty();
        var owned = await fixture.Repository.GetByOwnerIdAsync(shop.OwnerId);
        owned!.Id.Should().Be(shop.Id);
        owned.Status.Should().Be(status);
        db.Entry(owned).State.Should().Be(EntityState.Unchanged);
        (await fixture.Repository.GetByOwnerIdAsync(Id(2))).Should().BeNull();
        (await fixture.Repository.GetActiveByIdAsync(Id(3))).Should().BeNull();
    }

    [Fact]
    public async Task DeletedShopIsHiddenButStillReservesOwnerAndSlug()
    {
        using var fixture = new Fixture();
        var shop = new Shop(Id(1), "Shop", "reserved");
        shop.Approve();
        await fixture.Repository.AddAsync(shop);
        fixture.Db.Entry(shop).Property(s => s.DeletedAt).CurrentValue = Time;
        await fixture.Db.SaveChangesAsync();
        fixture.Db.ChangeTracker.Clear();

        (await fixture.Repository.GetByOwnerIdAsync(shop.OwnerId)).Should().BeNull();
        (await fixture.Repository.GetActiveByIdAsync(shop.Id)).Should().BeNull();
        (await fixture.Repository.ExistsByOwnerAsync(shop.OwnerId)).Should().BeTrue();
        (await fixture.Repository.ExistsBySlugAsync(shop.Slug)).Should().BeTrue();
        (await fixture.Repository.ExistsByOwnerAsync(Id(2))).Should().BeFalse();
        (await fixture.Repository.ExistsBySlugAsync("missing")).Should().BeFalse();
    }

    [Fact]
    public async Task RegistrationStagesAllWrites_AndSaveParticipatesInCallerTransaction()
    {
        using var fixture = new Fixture();
        var db = fixture.Db;
        await using var transaction = await db.Database.BeginTransactionAsync();
        var shop = new Shop(Id(1), "Shop", "shop");
        var verification = new ShopVerification(shop.Id, shop.OwnerId, "front?signature=original", "back", "license", "123456", "Bank", "Branch");
        await fixture.Repository.AddAsync(shop);
        await fixture.Repository.AddVerificationAsync(verification);
        await fixture.Repository.AddRegistrationOutboxAsync(new OutboxMessage("ShopRegisteredEvent", "{}"));

        (await db.Shops.CountAsync()).Should().Be(0);
        (await db.ShopVerifications.CountAsync()).Should().Be(0);
        (await db.OutboxMessages.CountAsync()).Should().Be(0);
        db.ChangeTracker.Entries().Should().HaveCount(3).And.OnlyContain(e => e.State == EntityState.Added);
        (await fixture.Repository.SaveRegistrationAsync()).Should().BeNull();
        db.ChangeTracker.Clear();
        (await db.Shops.SingleAsync()).Status.Should().Be(ShopStatus.Pending);
        var saved = await db.ShopVerifications.SingleAsync();
        saved.SubmittedBy.Should().Be(shop.OwnerId);
        saved.IdCardFront.Should().Be(verification.IdCardFront);
        saved.IdCardBack.Should().Be("back");
        saved.BusinessLicense.Should().Be("license");
        saved.BankAccount.Should().Be("123456");
        saved.BankName.Should().Be("Bank");
        saved.BankBranch.Should().Be("Branch");
        (await db.OutboxMessages.CountAsync()).Should().Be(1);

        await transaction.RollbackAsync();
        db.ChangeTracker.Clear();
        (await db.Shops.CountAsync()).Should().Be(0);
        (await db.ShopVerifications.CountAsync()).Should().Be(0);
        (await db.OutboxMessages.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task LatestVerificationUsesSubmittedTimeThenId_AndDoesNotTrackOrCrossShops()
    {
        using var fixture = new Fixture();
        var shopId = Id(10);
        foreach (var (id, ownerShop, day) in new[] { (1, shopId, 1), (2, shopId, 2), (3, shopId, 2), (4, Id(20), 3) })
        {
            var verification = new ShopVerification(ownerShop, Id(30), "front", "back", null, "12345", "Bank", null);
            fixture.Db.Entry(verification).Property(v => v.Id).CurrentValue = Id(id);
            fixture.Db.Entry(verification).Property(v => v.SubmittedAt).CurrentValue = Time.AddDays(day);
            await fixture.Repository.AddVerificationAsync(verification);
        }
        await fixture.Db.SaveChangesAsync();
        fixture.Db.ChangeTracker.Clear();

        (await fixture.Repository.GetLatestVerificationAsync(shopId))!.Id.Should().Be(Id(3));
        (await fixture.Repository.GetLatestVerificationAsync(Id(99))).Should().BeNull();
        fixture.Db.ChangeTracker.Entries().Should().BeEmpty();
    }

    [Fact]
    public async Task AddressQueriesPreserveDistinctOrderingTrackingAndShopScope()
    {
        using var fixture = new Fixture();
        var shopId = Id(10);
        foreach (var (id, ownerShop, day, primary) in new[]
                 { (1, shopId, 2, false), (2, shopId, 1, false), (3, shopId, 1, false), (4, shopId, 3, true), (5, Id(20), 0, true) })
        {
            var address = new ShopAddress(ownerShop, "Warehouse", "Contact", "0912345678", 1, "P", 2, "D", "00003", "W", "Street");
            fixture.Db.Entry(address).Property(a => a.Id).CurrentValue = Id(id);
            fixture.Db.Entry(address).Property(a => a.CreatedAt).CurrentValue = Time.AddDays(day);
            address.SetPrimary(primary);
            await fixture.Repository.AddAddressAsync(address);
        }
        (await fixture.Db.ShopAddresses.CountAsync()).Should().Be(0);
        await fixture.Db.SaveChangesAsync();
        fixture.Db.ChangeTracker.Clear();

        var editable = await fixture.Repository.GetAddressesAsync(shopId);
        editable.Select(a => a.Id).Should().BeEquivalentTo(new[] { Id(1), Id(2), Id(3), Id(4) });
        fixture.Db.ChangeTracker.Entries<ShopAddress>().Should().HaveCount(4);
        editable.Should().OnlyContain(a => a.ProvinceId == 1 && a.DistrictId == 2 && a.WardCode == "00003");
        fixture.Db.ChangeTracker.Clear();

        var deletionOrder = await fixture.Repository.GetAddressesForDeletionAsync(shopId);
        deletionOrder.Select(a => a.Id).Should().Equal(Id(2), Id(3), Id(1), Id(4));
        fixture.Db.ChangeTracker.Entries<ShopAddress>().Should().HaveCount(4);
        fixture.Db.ChangeTracker.Clear();

        (await fixture.Repository.GetSellerAddressesAsync(shopId)).Select(a => a.Id).Should().Equal(Id(4), Id(2), Id(3), Id(1));
        (await fixture.Repository.GetPublicAddressesAsync(shopId)).Select(a => a.Id).Should().Equal(Id(4), Id(1), Id(2), Id(3));
        (await fixture.Repository.GetAddressesAsync(Id(99))).Should().BeEmpty();
        fixture.Db.ChangeTracker.Entries().Should().BeEmpty();

        var toDelete = (await fixture.Repository.GetAddressesAsync(shopId)).Single(a => a.Id == Id(1));
        fixture.Repository.DeleteAddress(toDelete);
        (await fixture.Db.ShopAddresses.CountAsync()).Should().Be(5);
        await fixture.Db.SaveChangesAsync();
        (await fixture.Repository.GetAddressesAsync(shopId)).Should().HaveCount(3);
        (await fixture.Repository.GetAddressesAsync(Id(20))).Should().ContainSingle();
    }

    [Fact]
    public async Task FollowersAreScopedAndUnfollowIsAtomicIdempotentAndTransactional()
    {
        using var fixture = new Fixture();
        var repository = fixture.Repository;
        await repository.AddFollowerAsync(new ShopFollower(Id(1), Id(10)));
        await repository.AddFollowerAsync(new ShopFollower(Id(1), Id(20)));
        await repository.AddFollowerAsync(new ShopFollower(Id(2), Id(10)));
        (await repository.CountFollowersAsync(Id(1))).Should().Be(0);
        await repository.SaveFollowAsync();
        fixture.Db.ChangeTracker.Clear();

        (await repository.CountFollowersAsync(Id(1))).Should().Be(2);
        (await repository.CountFollowersAsync(Id(99))).Should().Be(0);
        (await repository.IsFollowingAsync(Id(1), Id(10))).Should().BeTrue();
        (await repository.IsFollowingAsync(Id(1), null)).Should().BeFalse();
        (await repository.IsFollowingAsync(Id(1), Id(30))).Should().BeFalse();
        await using (var transaction = await fixture.Db.Database.BeginTransactionAsync())
        {
            await repository.RemoveFollowerAsync(Id(1), Id(10));
            (await repository.CountFollowersAsync(Id(1))).Should().Be(1);
            await transaction.RollbackAsync();
        }
        (await repository.IsFollowingAsync(Id(1), Id(10))).Should().BeTrue();
        await repository.RemoveFollowerAsync(Id(1), Id(10));
        await repository.RemoveFollowerAsync(Id(1), Id(10));
        (await repository.CountFollowersAsync(Id(1))).Should().Be(1);
        (await repository.IsFollowingAsync(Id(1), Id(20))).Should().BeTrue();
        (await repository.IsFollowingAsync(Id(2), Id(10))).Should().BeTrue();
    }

    [Fact]
    public async Task ProductCountExcludesDraftDeletedAndOtherShopProducts()
    {
        using var fixture = new Fixture();
        var shopId = Id(1);
        var deleted = new Spu(shopId, Id(10), "Deleted", null, null, null, null);
        deleted.SoftDelete();
        fixture.Db.Spus.AddRange(new Spu(shopId, Id(10), "Active", null, null, null, null),
            new Spu(shopId, Id(10), "Draft", null, null, null, null, ProductStatus.Draft),
            new Spu(Id(2), Id(10), "Other shop", null, null, null, null), deleted);
        await fixture.Db.SaveChangesAsync();
        fixture.Db.ChangeTracker.Clear();
        var repository = new SpuRepository(fixture.Db);
        (await repository.CountActiveByShopAsync(shopId)).Should().Be(1);
        (await repository.CountActiveByShopAsync(Id(99))).Should().Be(0);
        fixture.Db.ChangeTracker.Entries().Should().BeEmpty();
    }

    [Fact]
    public async Task RegistrationUserLookupPreservesTrackingAndSoftDeleteFilter()
    {
        using var fixture = new Fixture();
        var user = new User("buyer@example.com", "hash", "Buyer", "0912345678");
        fixture.Db.Users.Add(user);
        await fixture.Db.SaveChangesAsync();
        fixture.Db.ChangeTracker.Clear();
        var repository = new UserRepository(fixture.Db);
        var found = await repository.GetTrackedByIdAsync(user.Id);
        found!.Id.Should().Be(user.Id);
        fixture.Db.Entry(found).State.Should().Be(EntityState.Unchanged);
        (await repository.GetTrackedByIdAsync(Id(99))).Should().BeNull();
        fixture.Db.Entry(found).Property(u => u.DeletedAt).CurrentValue = Time;
        await fixture.Db.SaveChangesAsync();
        fixture.Db.ChangeTracker.Clear();
        (await repository.GetTrackedByIdAsync(user.Id)).Should().BeNull();
    }

    private sealed class Fixture : IDisposable
    {
        private readonly SqliteConnection connection = new("Data Source=:memory:");
        public AppDbContext Db { get; }
        public ShopRepository Repository { get; }
        public Fixture()
        {
            connection.Open();
            Db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection)
                .UseSnakeCaseNamingConvention().ReplaceService<IModelCustomizer, SqliteTimeModelCustomizer>().Options);
            Db.Database.EnsureCreated();
            Repository = new ShopRepository(Db);
        }
        public void Dispose() { Db.Dispose(); connection.Dispose(); }
    }
}
