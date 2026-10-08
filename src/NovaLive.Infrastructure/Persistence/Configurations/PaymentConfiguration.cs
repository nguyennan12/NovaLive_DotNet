using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaLive.Domain.Payments;

namespace NovaLive.Infrastructure.Persistence.Configurations;

public sealed class PaymentConfiguration :
    IEntityTypeConfiguration<Payment>,
    IEntityTypeConfiguration<SellerPayout>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");
        builder.HasKey(payment => payment.Id);

        builder.Property(payment => payment.Method).HasConversion<string>().HasMaxLength(20);
        builder.Property(payment => payment.Amount).HasPrecision(18, 2);
        builder.Property(payment => payment.TransactionRef).HasMaxLength(200);
        builder.Property(payment => payment.GatewayResponse).HasColumnType("jsonb");
        builder.Property(payment => payment.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(payment => payment.ParentOrderId);
        builder.HasIndex(payment => payment.TransactionRef).IsUnique();
    }

    public void Configure(EntityTypeBuilder<SellerPayout> builder)
    {
        builder.ToTable("seller_payouts");
        builder.HasKey(payout => payout.Id);

        builder.Property(payout => payout.Amount).HasPrecision(18, 2);
        builder.Property(payout => payout.BankAccount).HasMaxLength(30).IsRequired();
        builder.Property(payout => payout.BankName).HasMaxLength(100).IsRequired();
        builder.Property(payout => payout.TransferRef).HasMaxLength(200);
        builder.Property(payout => payout.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(payout => new { payout.ShopId, payout.Status });
    }
}
