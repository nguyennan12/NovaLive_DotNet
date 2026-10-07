using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaLive.Domain.System;

namespace NovaLive.Infrastructure.Persistence.Configurations;

public sealed class SystemConfiguration :
    IEntityTypeConfiguration<AuditLog>,
    IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");
        builder.HasKey(log => log.Id);

        builder.Property(log => log.Action).HasMaxLength(80).IsRequired();
        builder.Property(log => log.EntityType).HasMaxLength(50).IsRequired();
        builder.Property(log => log.OldDataJson).HasColumnType("jsonb");
        builder.Property(log => log.NewDataJson).HasColumnType("jsonb");
        builder.Property(log => log.IpAddress).HasMaxLength(45);

        builder.HasIndex(log => new { log.EntityType, log.EntityId });
    }

    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");
        builder.HasKey(msg => msg.Id);

        builder.Property(msg => msg.EventType).HasMaxLength(200).IsRequired();
        builder.Property(msg => msg.Payload).HasColumnType("jsonb");

        builder.HasIndex(msg => new { msg.ProcessedAt, msg.CreatedAt }).HasFilter("processed_at IS NULL");
    }
}
