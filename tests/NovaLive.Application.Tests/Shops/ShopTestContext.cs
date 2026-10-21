using NovaLive.Application.Abstractions.Persistence.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Moq;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.UseCases.Shops;
using NovaLive.Domain.Common;
using NovaLive.Domain.Products;
using NovaLive.Domain.Rbac;
using NovaLive.Domain.Shops;
using NovaLive.Domain.System;
using NovaLive.Domain.Users;
namespace NovaLive.Application.Tests.Shops;

internal sealed class ShopTestContext : DbContext
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    public Mock<IUnitOfWork> UnitOfWork { get; } = new();
    public Mock<IUserRepository> Users { get; } = new();
    public Mock<ISpuRepository> Products { get; } = new();
    public Mock<ICurrentUser> User { get; } = new();
    public Mock<IShopRepository> Repository { get; } = new();
    public Mock<IAuditLogRepository> AuditLogs { get; } = new();
    public Mock<IDateTimeProvider> Clock { get; } = new();
    public ShopAccess Access => new(Repository.Object, User.Object);
    public ShopAddressDefaults Defaults => new(UnitOfWork.Object);
    public Guid UserId { get; } = Guid.NewGuid();

    public ShopTestContext()
    {
        connection.Open();
        Database.EnsureCreated();
        UnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns((CancellationToken ct) => SaveChangesAsync(ct));
        Users.Setup(x => x.GetTrackedByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns((Guid id, CancellationToken ct) => Set<User>().SingleOrDefaultAsync(u => u.Id == id, ct));
        Products.Setup(x => x.CountActiveByShopAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns((Guid id, CancellationToken ct) => Set<Spu>().CountAsync(p => p.ShopId == id && p.Status == ProductStatus.Active && p.DeletedAt == null, ct));
        Repository.Setup(x => x.GetByOwnerIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns((Guid id, CancellationToken ct) => Set<Shop>().SingleOrDefaultAsync(s => s.OwnerId == id && s.DeletedAt == null, ct));
        Repository.Setup(x => x.GetActiveByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns((Guid id, CancellationToken ct) => Set<Shop>().AsNoTracking().SingleOrDefaultAsync(s => s.Id == id && s.Status == ShopStatus.Active && s.DeletedAt == null, ct));
        Repository.Setup(x => x.GetTrackedByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns((Guid id, CancellationToken ct) => Set<Shop>().SingleOrDefaultAsync(s => s.Id == id && s.DeletedAt == null, ct));
        Repository.Setup(x => x.GetPagedAsync(It.IsAny<ShopStatus?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(async (ShopStatus? status, int page, int size, CancellationToken ct) =>
            {
                var query = Set<Shop>().AsNoTracking().Where(s => s.DeletedAt == null);
                if (status.HasValue) query = query.Where(s => s.Status == status.Value);
                var total = await query.LongCountAsync(ct);
                var items = await query.OrderByDescending(s => s.CreatedAt).ThenByDescending(s => s.Id).Skip((page - 1) * size).Take(size).ToListAsync(ct);
                return (items, total);
            });
        Products.Setup(x => x.DeactivateByShopIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(async (Guid shopId, CancellationToken ct) =>
            {
                var activeSpus = await Set<Spu>().Where(s => s.ShopId == shopId && s.Status == ProductStatus.Active).ToListAsync(ct);
                foreach (var spu in activeSpus) spu.SetStatus(ProductStatus.Inactive);
                await SaveChangesAsync(ct);
            });
        Repository.Setup(x => x.ExistsByOwnerAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns((Guid id, CancellationToken ct) => Set<Shop>().IgnoreQueryFilters().AnyAsync(s => s.OwnerId == id, ct));
        Repository.Setup(x => x.ExistsBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns((string slug, CancellationToken ct) => Set<Shop>().IgnoreQueryFilters().AnyAsync(s => s.Slug == slug, ct));
        Repository.Setup(x => x.AddAsync(It.IsAny<Shop>(), It.IsAny<CancellationToken>()))
            .Returns(async (Shop shop, CancellationToken ct) => { await Set<Shop>().AddAsync(shop, ct); });
        Repository.Setup(x => x.AddVerificationAsync(It.IsAny<ShopVerification>(), It.IsAny<CancellationToken>()))
            .Returns(async (ShopVerification verification, CancellationToken ct) => { await Set<ShopVerification>().AddAsync(verification, ct); });
        Repository.Setup(x => x.AddRegistrationOutboxAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()))
            .Returns(async (OutboxMessage message, CancellationToken ct) => { await Set<OutboxMessage>().AddAsync(message, ct); });
        Repository.Setup(x => x.GetLatestVerificationAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns((Guid id, CancellationToken ct) => Set<ShopVerification>().AsNoTracking().Where(v => v.ShopId == id)
                .OrderByDescending(v => v.SubmittedAt).ThenByDescending(v => v.Id).FirstOrDefaultAsync(ct));
        Repository.Setup(x => x.GetLatestTrackedVerificationAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns((Guid id, CancellationToken ct) => Set<ShopVerification>().Where(v => v.ShopId == id)
                .OrderByDescending(v => v.SubmittedAt).ThenByDescending(v => v.Id).FirstOrDefaultAsync(ct));
        Repository.Setup(x => x.GetAddressesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns((Guid id, CancellationToken ct) => Set<ShopAddress>().Where(a => a.ShopId == id).ToListAsync(ct));
        Repository.Setup(x => x.GetAddressesForDeletionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns((Guid id, CancellationToken ct) => Set<ShopAddress>().Where(a => a.ShopId == id)
                .OrderBy(a => a.CreatedAt).ThenBy(a => a.Id).ToListAsync(ct));
        Repository.Setup(x => x.GetSellerAddressesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns((Guid id, CancellationToken ct) => Set<ShopAddress>().AsNoTracking().Where(a => a.ShopId == id)
                .OrderByDescending(a => a.IsPrimary).ThenBy(a => a.CreatedAt).ThenBy(a => a.Id).ToListAsync(ct));
        Repository.Setup(x => x.GetPublicAddressesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns((Guid id, CancellationToken ct) => Set<ShopAddress>().AsNoTracking().Where(a => a.ShopId == id)
                .OrderByDescending(a => a.IsPrimary).ThenBy(a => a.Id).ToListAsync(ct));
        Repository.Setup(x => x.AddAddressAsync(It.IsAny<ShopAddress>(), It.IsAny<CancellationToken>()))
            .Returns(async (ShopAddress address, CancellationToken ct) => { await Set<ShopAddress>().AddAsync(address, ct); });
        Repository.Setup(x => x.DeleteAddress(It.IsAny<ShopAddress>())).Callback((ShopAddress address) => Set<ShopAddress>().Remove(address));
        Repository.Setup(x => x.CountFollowersAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns((Guid id, CancellationToken ct) => Set<ShopFollower>().CountAsync(f => f.ShopId == id, ct));
        Repository.Setup(x => x.IsFollowingAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .Returns((Guid id, Guid? userId, CancellationToken ct) => Set<ShopFollower>().AnyAsync(f => f.ShopId == id && f.UserId == userId, ct));
        Repository.Setup(x => x.AddFollowerAsync(It.IsAny<ShopFollower>(), It.IsAny<CancellationToken>()))
            .Returns(async (ShopFollower follower, CancellationToken ct) => { await Set<ShopFollower>().AddAsync(follower, ct); });
        Repository.Setup(x => x.RemoveFollowerAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns((Guid id, Guid userId, CancellationToken ct) => Set<ShopFollower>().Where(f => f.ShopId == id && f.UserId == userId).ExecuteDeleteAsync(ct));
        User.SetupGet(x => x.UserId).Returns(UserId);
        User.SetupGet(x => x.IsAuthenticated).Returns(true);
        Repository.Setup(x => x.SaveRegistrationAsync(It.IsAny<CancellationToken>()))
            .Returns(async (CancellationToken ct) => { await SaveChangesAsync(ct); return (Error?)null; });
        Repository.Setup(x => x.SaveFollowAsync(It.IsAny<CancellationToken>()))
            .Returns((CancellationToken ct) => SaveChangesAsync(ct));
        Clock.SetupGet(x => x.UtcNow).Returns(new DateTimeOffset(2026, 10, 21, 14, 0, 0, TimeSpan.Zero));
    }
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<ShopWallet> ShopWallets => Set<ShopWallet>();

    protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite(connection);
    protected override void OnModelCreating(ModelBuilder model)
    {
        foreach (var type in new[] { typeof(User), typeof(Shop), typeof(ShopVerification), typeof(ShopFollower),
                     typeof(ShopAddress), typeof(Spu), typeof(OutboxMessage), typeof(UserRole), typeof(ShopWallet) })
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
