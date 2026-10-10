using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaLive.Domain.Rbac;

namespace NovaLive.Infrastructure.Persistence.Configurations;

public sealed class RbacConfiguration :
    IEntityTypeConfiguration<Role>,
    IEntityTypeConfiguration<Resource>,
    IEntityTypeConfiguration<Permission>,
    IEntityTypeConfiguration<RolePermission>,
    IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name).HasMaxLength(100).IsRequired();
        builder.Property(r => r.Description).HasMaxLength(500);

        builder.HasIndex(r => r.Name).IsUnique();
    }

    public void Configure(EntityTypeBuilder<Resource> builder)
    {
        builder.ToTable("resources");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Code).HasMaxLength(80).IsRequired();
        builder.Property(r => r.Description).HasMaxLength(300);

        builder.HasIndex(r => r.Code).IsUnique();
    }

    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("permissions");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Action).HasMaxLength(30).IsRequired();
        builder.Property(p => p.Code).HasMaxLength(120).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(300);

        builder.HasIndex(p => p.Code).IsUnique();
        builder.HasIndex(p => new { p.ResourceId, p.Action }).IsUnique();
    }

    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("role_permissions");
        builder.HasKey(rp => rp.Id);

        builder.HasIndex(rp => new { rp.RoleId, rp.PermissionId }).IsUnique();
    }

    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("user_roles");
        builder.HasKey(ur => ur.Id);

        builder.HasIndex(ur => new { ur.UserId, ur.RoleId }).IsUnique();
        builder.HasIndex(ur => ur.UserId);
    }
}
