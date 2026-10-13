using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Moq;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.UseCases.Shops;
using NovaLive.Domain.Common;
using NovaLive.Domain.Products;
using NovaLive.Domain.Shops;
using NovaLive.Domain.System;
using NovaLive.Domain.Users;
namespace NovaLive.Application.Tests.Shops;

internal sealed class ShopTestContext : DbContext
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    public Mock<IAppDbContext> App { get; } = new();
    public Mock<ICurrentUser> User { get; } = new();
    public Mock<IShopPersistence> Persistence { get; } = new();
    public ShopAccess Access => new(App.Object, User.Object, Persistence.Object);
    public ShopAddressDefaults Defaults => new(App.Object);
    public Guid UserId { get; } = Guid.NewGuid();

    public ShopTestContext()
    {
        connection.Open();
        Database.EnsureCreated();
        App.SetupGet(x => x.Users).Returns(Set<User>());
        App.SetupGet(x => x.Shops).Returns(Set<Shop>());
        App.SetupGet(x => x.ShopVerifications).Returns(Set<ShopVerification>());
        App.SetupGet(x => x.ShopFollowers).Returns(Set<ShopFollower>());
        App.SetupGet(x => x.ShopAddresses).Returns(Set<ShopAddress>());
        App.SetupGet(x => x.Spus).Returns(Set<Spu>());
        App.SetupGet(x => x.OutboxMessages).Returns(Set<OutboxMessage>());
        App.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns((CancellationToken ct) => SaveChangesAsync(ct));
        User.SetupGet(x => x.UserId).Returns(UserId);
        User.SetupGet(x => x.IsAuthenticated).Returns(true);
        Persistence.Setup(x => x.SaveRegistrationAsync(It.IsAny<CancellationToken>()))
            .Returns(async (CancellationToken ct) => { await SaveChangesAsync(ct); return (Error?)null; });
        Persistence.Setup(x => x.SaveFollowAsync(It.IsAny<CancellationToken>()))
            .Returns((CancellationToken ct) => SaveChangesAsync(ct));
        Persistence.Setup(x => x.RemoveFollowAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns((Guid shop, Guid user, CancellationToken ct) => Set<ShopFollower>()
                .Where(f => f.ShopId == shop && f.UserId == user).ExecuteDeleteAsync(ct));
    }
    protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite(connection);
    protected override void OnModelCreating(ModelBuilder model)
    {
        foreach (var type in new[] { typeof(User), typeof(Shop), typeof(ShopVerification), typeof(ShopFollower),
                     typeof(ShopAddress), typeof(Spu), typeof(OutboxMessage) })
            model.Entity(type).Ignore(nameof(Entity.DomainEvents));
        model.Entity<Shop>().HasQueryFilter(s => s.DeletedAt == null);
        model.Entity<User>().HasQueryFilter(u => u.DeletedAt == null);
        model.Entity<Spu>().HasQueryFilter(p => p.DeletedAt == null);
        foreach (var entity in model.Model.GetEntityTypes())
        foreach (var property in entity.GetProperties())
            if (property.ClrType == typeof(DateTimeOffset) || property.ClrType == typeof(DateTimeOffset?))
                property.SetValueConverter(new DateTimeOffsetToBinaryConverter());
    }
    public async Task<User> AddUser(bool active = true)
    {
        var user = new User("buyer@example.com", "hash", "Buyer", "0912345678");
        Entry(user).Property(x => x.Id).CurrentValue = UserId;
        if (active) user.Activate();
        Add(user); await SaveChangesAsync(); return user;
    }
    public async Task<Shop> AddShop(Guid? owner = null, ShopStatus status = ShopStatus.Pending)
    {
        var shop = new Shop(owner ?? UserId, "Shop", Guid.NewGuid().ToString());
        Entry(shop).Property(s => s.Status).CurrentValue = status;
        Add(shop); await SaveChangesAsync(); return shop;
    }
    public override void Dispose() { base.Dispose(); connection.Dispose(); }
}
