using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaLive.Domain.Users;

namespace NovaLive.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration :
    IEntityTypeConfiguration<User>,
    IEntityTypeConfiguration<UserAddress>,
    IEntityTypeConfiguration<UserOtp>,
    IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(user => user.Id);

        builder.Property(user => user.Email).HasMaxLength(255).IsRequired();
        builder.Property(user => user.Phone).HasMaxLength(20);
        builder.Property(user => user.PasswordHash).HasMaxLength(255).IsRequired();
        builder.Property(user => user.FullName).HasMaxLength(200).IsRequired();
        builder.Property(user => user.Gender).HasConversion<string>().HasMaxLength(20);
        builder.Property(user => user.AvatarUrl).HasMaxLength(500);
        builder.Property(user => user.AccountStatus).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(user => user.Email).IsUnique().HasFilter("deleted_at IS NULL");
        builder.HasIndex(user => user.Phone).IsUnique().HasFilter("phone IS NOT NULL AND deleted_at IS NULL");
    }

    public void Configure(EntityTypeBuilder<UserAddress> builder)
    {
        builder.ToTable("user_addresses");
        builder.HasKey(addr => addr.Id);

        builder.Property(addr => addr.RecipientName).HasMaxLength(150).IsRequired();
        builder.Property(addr => addr.Phone).HasMaxLength(20).IsRequired();
        builder.Property(addr => addr.ProvinceName).HasMaxLength(100).IsRequired();
        builder.Property(addr => addr.DistrictName).HasMaxLength(100).IsRequired();
        builder.Property(addr => addr.WardCode).HasMaxLength(20);
        builder.Property(addr => addr.WardName).HasMaxLength(100).IsRequired();
        builder.Property(addr => addr.DetailAddress).HasMaxLength(300).IsRequired();

        builder.HasIndex(addr => addr.UserId);
    }

    public void Configure(EntityTypeBuilder<UserOtp> builder)
    {
        builder.ToTable("user_otps");
        builder.HasKey(otp => otp.Id);

        builder.Property(otp => otp.OtpType).HasConversion<string>().HasMaxLength(30);
        builder.Property(otp => otp.OtpHash).HasMaxLength(255).IsRequired();

        builder.HasIndex(otp => new { otp.UserId, otp.OtpType, otp.ExpiresAt });
    }

    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");
        builder.HasKey(token => token.Id);

        builder.Property(token => token.TokenHash).HasMaxLength(255).IsRequired();
        builder.Property(token => token.DeviceInfo).HasMaxLength(500);
        builder.Property(token => token.IpAddress).HasMaxLength(45);

        builder.HasIndex(token => token.TokenHash).IsUnique();
        builder.HasIndex(token => new { token.UserId, token.RevokedAt });
    }
}
