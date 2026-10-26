using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Moq;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.UseCases.Shops;
using NovaLive.Domain.Common;
using NovaLive.Domain.Payments;
using NovaLive.Domain.Shops;
using NovaLive.Domain.Users;

namespace NovaLive.Application.Tests.Wallets;

internal sealed class WalletTestContext : DbContext
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    public Mock<ICurrentUser> CurrentUser { get; } = new();
    public Mock<IShopRepository> ShopRepository { get; } = new();
    public Mock<IShopWalletRepository> WalletRepository { get; } = new();
    public Mock<IShopWalletTransactionRepository> TransactionRepository { get; } = new();
    public Mock<ISellerPayoutRepository> PayoutRepository { get; } = new();
    public Mock<IUserRepository> UserRepository { get; } = new();
    public Mock<IDateTimeProvider> Clock { get; } = new();

    public ShopAccess Access => new(ShopRepository.Object, CurrentUser.Object);
    public Guid UserId { get; } = Guid.NewGuid();
    public DateTimeOffset Now { get; set; } = new(2026, 10, 21, 12, 0, 0, TimeSpan.Zero);

    public WalletTestContext()
    {
        _connection.Open();
        Database.EnsureCreated();

        CurrentUser.SetupGet(x => x.UserId).Returns(UserId);
        CurrentUser.SetupGet(x => x.IsAuthenticated).Returns(true);
        CurrentUser.Setup(x => x.IsInRole("Seller")).Returns(true);

        Clock.SetupGet(x => x.UtcNow).Returns(() => Now);

        ShopRepository.Setup(x => x.GetByOwnerIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns((Guid id, CancellationToken ct) => Set<Shop>().SingleOrDefaultAsync(s => s.OwnerId == id && s.DeletedAt == null, ct));

        ShopRepository.Setup(x => x.GetLatestVerificationAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns((Guid id, CancellationToken ct) => Set<ShopVerification>().AsNoTracking().Where(v => v.ShopId == id)
                .OrderByDescending(v => v.SubmittedAt).ThenByDescending(v => v.Id).FirstOrDefaultAsync(ct));

        WalletRepository.Setup(x => x.GetByShopIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns((Guid shopId, CancellationToken ct) => Set<ShopWallet>().FirstOrDefaultAsync(w => w.ShopId == shopId, ct));

        WalletRepository.Setup(x => x.GetByShopIdForUpdateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns((Guid shopId, CancellationToken ct) => Set<ShopWallet>().FirstOrDefaultAsync(w => w.ShopId == shopId, ct));

        WalletRepository.Setup(x => x.AddAsync(It.IsAny<ShopWallet>(), It.IsAny<CancellationToken>()))
            .Returns(async (ShopWallet wallet, CancellationToken ct) => { await Set<ShopWallet>().AddAsync(wallet, ct); await SaveChangesAsync(ct); });

        WalletRepository.Setup(x => x.AddTransactionAsync(It.IsAny<ShopWalletTransaction>(), It.IsAny<CancellationToken>()))
            .Returns(async (ShopWalletTransaction tx, CancellationToken ct) => { await Set<ShopWalletTransaction>().AddAsync(tx, ct); await SaveChangesAsync(ct); });

        TransactionRepository.Setup(x => x.AddAsync(It.IsAny<ShopWalletTransaction>(), It.IsAny<CancellationToken>()))
            .Returns(async (ShopWalletTransaction tx, CancellationToken ct) => { await Set<ShopWalletTransaction>().AddAsync(tx, ct); await SaveChangesAsync(ct); });

        TransactionRepository.Setup(x => x.GetPagedAsync(
                It.IsAny<Guid>(),
                It.IsAny<WalletTxType?>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .Returns(async (Guid walletId, WalletTxType? type, DateTimeOffset? from, DateTimeOffset? to, int page, int size, CancellationToken ct) =>
            {
                var query = Set<ShopWalletTransaction>().AsNoTracking().Where(t => t.WalletId == walletId);
                if (type.HasValue) query = query.Where(t => t.Type == type.Value);
                if (from.HasValue) query = query.Where(t => t.CreatedAt >= from.Value);
                if (to.HasValue) query = query.Where(t => t.CreatedAt <= to.Value);
                var total = await query.LongCountAsync(ct);
                var items = await query.OrderByDescending(t => t.CreatedAt).ThenByDescending(t => t.Id).Skip((page - 1) * size).Take(size).ToListAsync(ct);
                return (items, total);
            });

        PayoutRepository.Setup(x => x.AddAsync(It.IsAny<SellerPayout>(), It.IsAny<CancellationToken>()))
            .Returns(async (SellerPayout payout, CancellationToken ct) => { await Set<SellerPayout>().AddAsync(payout, ct); await SaveChangesAsync(ct); });

        PayoutRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns((Guid id, CancellationToken ct) => Set<SellerPayout>().FirstOrDefaultAsync(p => p.Id == id, ct));

        PayoutRepository.Setup(x => x.GetByIdForUpdateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns((Guid id, CancellationToken ct) => Set<SellerPayout>().FirstOrDefaultAsync(p => p.Id == id, ct));

        PayoutRepository.Setup(x => x.GetPagedAsync(
                It.IsAny<Guid?>(),
                It.IsAny<PayoutStatus?>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .Returns(async (Guid? shopId, PayoutStatus? status, int page, int size, CancellationToken ct) =>
            {
                var query = Set<SellerPayout>().AsNoTracking();
                if (shopId.HasValue) query = query.Where(p => p.ShopId == shopId.Value);
                if (status.HasValue) query = query.Where(p => p.Status == status.Value);
                var total = await query.LongCountAsync(ct);
                var items = await query.OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.Id).Skip((page - 1) * size).Take(size).ToListAsync(ct);
                return (items, total);
            });

        ShopRepository.Setup(x => x.GetTrackedByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns((Guid id, CancellationToken ct) => Set<Shop>().FirstOrDefaultAsync(s => s.Id == id && s.DeletedAt == null, ct));

        UserRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns((Guid id, CancellationToken ct) => Set<User>().FirstOrDefaultAsync(u => u.Id == id, ct));
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite(_connection);

    protected override void OnModelCreating(ModelBuilder model)
    {
        foreach (var type in new[] { typeof(User), typeof(Shop), typeof(ShopVerification), typeof(ShopWallet), typeof(ShopWalletTransaction), typeof(SellerPayout) })
            model.Entity(type).Ignore(nameof(Entity.DomainEvents));

        model.Entity<Shop>().HasQueryFilter(s => s.DeletedAt == null);
        model.Entity<User>().HasQueryFilter(u => u.DeletedAt == null);

        foreach (var entity in model.Model.GetEntityTypes())
        foreach (var property in entity.GetProperties())
            if (property.ClrType == typeof(DateTimeOffset) || property.ClrType == typeof(DateTimeOffset?))
                property.SetValueConverter(new DateTimeOffsetToBinaryConverter());
    }

    public async Task<User> AddUser(string fullName = "Nguyễn Văn Chủ Shop")
    {
        var user = new User("seller@example.com", "hash", fullName, "0912345678");
        Entry(user).Property(x => x.Id).CurrentValue = UserId;
        user.Activate();
        user.MarkAsSeller();
        Add(user);
        await SaveChangesAsync();
        return user;
    }

    public async Task<Shop> AddShop(ShopStatus status = ShopStatus.Active)
    {
        var shop = new Shop(UserId, "Test Shop", "test-shop");
        if (status == ShopStatus.Active) shop.Approve();
        else if (status == ShopStatus.Suspended) shop.Suspend();
        Add(shop);
        await SaveChangesAsync();
        return shop;
    }

    public async Task<ShopVerification> AddKyc(Guid shopId, VerificationStatus status = VerificationStatus.Approved)
    {
        var kyc = new ShopVerification(
            shopId,
            UserId,
            "https://cdn.example.com/id_front.jpg",
            "https://cdn.example.com/id_back.jpg",
            null,
            "0123456789",
            "Vietcombank",
            "Hà Nội");

        if (status == VerificationStatus.Approved)
        {
            Entry(kyc).Property(x => x.Status).CurrentValue = VerificationStatus.Approved;
        }

        Add(kyc);
        await SaveChangesAsync();
        return kyc;
    }

    public async Task<ShopWallet> AddWallet(Guid shopId, decimal balance = 500_000m, decimal lockedBalance = 0m)
    {
        var wallet = new ShopWallet(shopId);
        if (balance > 0) wallet.Credit(balance, Now);
        if (lockedBalance > 0)
        {
            wallet.Credit(lockedBalance, Now);
            wallet.LockPayout(lockedBalance, Now);
        }
        Add(wallet);
        await SaveChangesAsync();
        return wallet;
    }

    public async Task<SellerPayout> AddPayout(Guid shopId, decimal amount = 100_000m, PayoutStatus status = PayoutStatus.Pending)
    {
        var payout = SellerPayout.Create(shopId, amount, "0123456789", "Vietcombank", "Nguyễn Văn Chủ Shop");
        if (status == PayoutStatus.Completed) payout.Approve("TXN-123", Guid.NewGuid(), Now);
        else if (status == PayoutStatus.Failed) payout.Reject("Invalid account", Guid.NewGuid(), Now);
        Add(payout);
        await SaveChangesAsync();
        return payout;
    }

    public override void Dispose()
    {
        base.Dispose();
        _connection.Dispose();
    }
}
