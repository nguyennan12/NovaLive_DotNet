using Microsoft.EntityFrameworkCore;
using NovaLive.Domain.Carts;
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

namespace NovaLive.Application.Abstractions.Persistence;

public interface IAppDbContext
{
    // 1. Core
    DbSet<User> Users { get; }
    DbSet<UserAddress> UserAddresses { get; }
    DbSet<UserOtp> UserOtps { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    // 2. Shop & Wallet
    DbSet<Shop> Shops { get; }
    DbSet<ShopVerification> ShopVerifications { get; }
    DbSet<ShopAddress> ShopAddresses { get; }
    DbSet<ShopWallet> ShopWallets { get; }
    DbSet<ShopWalletTransaction> ShopWalletTransactions { get; }
    DbSet<ShopFollower> ShopFollowers { get; }

    // 3. RBAC
    DbSet<Role> Roles { get; }
    DbSet<Resource> Resources { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<UserRole> UserRoles { get; }

    // 4. Product
    DbSet<Category> Categories { get; }
    DbSet<Spu> Spus { get; }
    DbSet<Sku> Skus { get; }
    DbSet<SkuImage> SkuImages { get; }
    DbSet<ProductAttribute> ProductAttributes { get; }

    // 5. Inventory
    DbSet<Inventory> Inventories { get; }
    DbSet<InventoryHistory> InventoryHistories { get; }

    // 6. Cart
    DbSet<Cart> Carts { get; }
    DbSet<CartItem> CartItems { get; }

    // 7. Order
    DbSet<ParentOrder> ParentOrders { get; }
    DbSet<SubOrder> SubOrders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<OrderStatusHistory> OrderStatusHistories { get; }
    DbSet<OrderReturn> OrderReturns { get; }
    DbSet<OrderReturnItem> OrderReturnItems { get; }

    // 8. Discount
    DbSet<Discount> Discounts { get; }
    DbSet<DiscountUsage> DiscountUsages { get; }

    // 9. Flash Sale
    DbSet<FlashSaleCampaign> FlashSaleCampaigns { get; }
    DbSet<FlashSaleItem> FlashSaleItems { get; }

    // 10. Payment & Escrow
    DbSet<Payment> Payments { get; }
    DbSet<PaymentEscrow> PaymentEscrows { get; }
    DbSet<SellerPayout> SellerPayouts { get; }

    // 11. Shipping
    DbSet<ShippingOrder> ShippingOrders { get; }

    // 12. Livestream
    DbSet<LivestreamSession> LivestreamSessions { get; }
    DbSet<LivestreamProduct> LivestreamProducts { get; }
    DbSet<LivestreamComment> LivestreamComments { get; }

    // 13. Review
    DbSet<Review> Reviews { get; }
    DbSet<ReviewImage> ReviewImages { get; }

    // 14. Notification
    DbSet<Notification> Notifications { get; }

    // 15. System & EDA
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<OutboxMessage> OutboxMessages { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
