using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaLive.Domain.Shops;

namespace NovaLive.Infrastructure.Persistence.Configurations;

public sealed class ShopConfiguration :
    IEntityTypeConfiguration<Shop>,
    IEntityTypeConfiguration<ShopVerification>,
    IEntityTypeConfiguration<ShopAddress>,
    IEntityTypeConfiguration<ShopWallet>,
    IEntityTypeConfiguration<ShopWalletTransaction>,
    IEntityTypeConfiguration<ShopFollower>
{
    public void Configure(EntityTypeBuilder<Shop> builder)
    {
        builder.ToTable("shops");
        builder.HasKey(shop => shop.Id);

        builder.Property(shop => shop.ShopName).HasMaxLength(200).IsRequired();
        builder.Property(shop => shop.Slug).HasMaxLength(200).IsRequired();
        builder.Property(shop => shop.LogoUrl).HasMaxLength(500);
        builder.Property(shop => shop.BannerUrl).HasMaxLength(500);
        builder.Property(shop => shop.TaxCode).HasMaxLength(30);
        builder.Property(shop => shop.Phone).HasMaxLength(20);
        builder.Property(shop => shop.Email).HasMaxLength(255);
        builder.Property(shop => shop.RatingAvg).HasPrecision(3, 2);
        builder.Property(shop => shop.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(shop => shop.OwnerId).IsUnique();
        builder.HasIndex(shop => shop.Slug).IsUnique();
        builder.HasIndex(shop => shop.Status).HasFilter("deleted_at IS NULL");
    }

    public void Configure(EntityTypeBuilder<ShopVerification> builder)
    {
        builder.ToTable("shop_verifications");
        builder.HasKey(sv => sv.Id);

        builder.Property(sv => sv.IdCardFront).HasMaxLength(500).IsRequired();
        builder.Property(sv => sv.IdCardBack).HasMaxLength(500).IsRequired();
        builder.Property(sv => sv.BusinessLicense).HasMaxLength(500);
        builder.Property(sv => sv.BankAccount).HasMaxLength(30).IsRequired();
        builder.Property(sv => sv.BankName).HasMaxLength(100).IsRequired();
        builder.Property(sv => sv.BankBranch).HasMaxLength(200);
        builder.Property(sv => sv.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(sv => sv.ShopId);
        builder.HasIndex(sv => sv.Status).HasFilter("status = 'Pending'");
    }

    public void Configure(EntityTypeBuilder<ShopAddress> builder)
    {
        builder.ToTable("shop_addresses");
        builder.HasKey(addr => addr.Id);

        builder.Property(addr => addr.WarehouseName).HasMaxLength(150).IsRequired();
        builder.Property(addr => addr.ContactName).HasMaxLength(150).IsRequired();
        builder.Property(addr => addr.ContactPhone).HasMaxLength(20).IsRequired();
        builder.Property(addr => addr.ProvinceName).HasMaxLength(100).IsRequired();
        builder.Property(addr => addr.DistrictName).HasMaxLength(100).IsRequired();
        builder.Property(addr => addr.WardCode).HasMaxLength(20);
        builder.Property(addr => addr.WardName).HasMaxLength(100).IsRequired();
        builder.Property(addr => addr.DetailAddress).HasMaxLength(300).IsRequired();

        builder.HasIndex(addr => addr.ShopId);
    }

    public void Configure(EntityTypeBuilder<ShopWallet> builder)
    {
        builder.ToTable("shop_wallets");
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Balance).HasPrecision(18, 2);
        builder.Property(w => w.LockedBalance).HasPrecision(18, 2);
        builder.Property(w => w.Currency).HasMaxLength(3).IsRequired();

        builder.HasIndex(w => w.ShopId).IsUnique();
    }

    public void Configure(EntityTypeBuilder<ShopWalletTransaction> builder)
    {
        builder.ToTable("shop_wallet_transactions");
        builder.HasKey(tx => tx.Id);

        builder.Property(tx => tx.Type).HasConversion<string>().HasMaxLength(30);
        builder.Property(tx => tx.Amount).HasPrecision(18, 2);
        builder.Property(tx => tx.BalanceBefore).HasPrecision(18, 2);
        builder.Property(tx => tx.BalanceAfter).HasPrecision(18, 2);
        builder.Property(tx => tx.RefType).HasMaxLength(30);

        builder.HasIndex(tx => new { tx.WalletId, tx.CreatedAt });
        builder.HasIndex(tx => new { tx.RefType, tx.RefId });
    }

    public void Configure(EntityTypeBuilder<ShopFollower> builder)
    {
        builder.ToTable("shop_followers");
        builder.HasKey(sf => sf.Id);

        builder.HasIndex(sf => new { sf.ShopId, sf.UserId }).IsUnique();
        builder.HasIndex(sf => new { sf.ShopId, sf.CreatedAt });
        builder.HasIndex(sf => new { sf.UserId, sf.CreatedAt });
    }
}
