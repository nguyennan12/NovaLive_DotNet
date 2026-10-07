using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Domain.Carts;
using NovaLive.Domain.Common;
using NovaLive.Domain.Discounts;
using NovaLive.Domain.FlashSales;
using NovaLive.Domain.Inventory;
using NovaLive.Domain.Livestreams;
using NovaLive.Domain.Notifications;
using NovaLive.Domain.Orders;
using NovaLive.Domain.Payments;
using NovaLive.Domain.Products;
using NovaLive.Domain.Rbac;
using NovaLive.Domain.Reviews;
using NovaLive.Domain.Shipping;
using NovaLive.Domain.Shops;
using NovaLive.Domain.System;
using NovaLive.Domain.Users;

namespace NovaLive.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext, IUnitOfWork
{
    // 1. Core
    public DbSet<User> Users => Set<User>();
    public DbSet<UserAddress> UserAddresses => Set<UserAddress>();
    public DbSet<UserOtp> UserOtps => Set<UserOtp>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // 2. Shop & Wallet
    public DbSet<Shop> Shops => Set<Shop>();
    public DbSet<ShopVerification> ShopVerifications => Set<ShopVerification>();
    public DbSet<ShopAddress> ShopAddresses => Set<ShopAddress>();
    public DbSet<ShopWallet> ShopWallets => Set<ShopWallet>();
    public DbSet<ShopWalletTransaction> ShopWalletTransactions => Set<ShopWalletTransaction>();
    public DbSet<ShopFollower> ShopFollowers => Set<ShopFollower>();

    // 3. RBAC
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();

    // 4. Product
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Spu> Spus => Set<Spu>();
    public DbSet<Sku> Skus => Set<Sku>();
    public DbSet<SkuImage> SkuImages => Set<SkuImage>();
    public DbSet<ProductAttribute> ProductAttributes => Set<ProductAttribute>();

    // 5. Inventory
    public DbSet<Inventory> Inventories => Set<Inventory>();
    public DbSet<InventoryHistory> InventoryHistories => Set<InventoryHistory>();

    // 6. Cart
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();

    // 7. Order
    public DbSet<ParentOrder> ParentOrders => Set<ParentOrder>();
    public DbSet<SubOrder> SubOrders => Set<SubOrder>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
    public DbSet<OrderReturn> OrderReturns => Set<OrderReturn>();
    public DbSet<OrderReturnItem> OrderReturnItems => Set<OrderReturnItem>();

    // 8. Discount
    public DbSet<Discount> Discounts => Set<Discount>();
    public DbSet<DiscountUsage> DiscountUsages => Set<DiscountUsage>();

    // 9. Flash Sale
    public DbSet<FlashSaleCampaign> FlashSaleCampaigns => Set<FlashSaleCampaign>();
    public DbSet<FlashSaleItem> FlashSaleItems => Set<FlashSaleItem>();

    // 10. Payment & Escrow
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentEscrow> PaymentEscrows => Set<PaymentEscrow>();
    public DbSet<SellerPayout> SellerPayouts => Set<SellerPayout>();

    // 11. Shipping
    public DbSet<ShippingOrder> ShippingOrders => Set<ShippingOrder>();

    // 12. Livestream
    public DbSet<LivestreamSession> LivestreamSessions => Set<LivestreamSession>();
    public DbSet<LivestreamProduct> LivestreamProducts => Set<LivestreamProduct>();
    public DbSet<LivestreamComment> LivestreamComments => Set<LivestreamComment>();

    // 13. Review
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<ReviewImage> ReviewImages => Set<ReviewImage>();

    // 14. Notification
    public DbSet<Notification> Notifications => Set<Notification>();

    // 15. System & EDA
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
                var notDeleted = Expression.Not(property);
                var lambda = Expression.Lambda(notDeleted, parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }
}
