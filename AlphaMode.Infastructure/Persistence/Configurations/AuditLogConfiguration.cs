using AlphaMode.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Action)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.EntityName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.EntityId)
            .HasMaxLength(100);

        builder.Property(a => a.UserId)
            .HasMaxLength(450);

        builder.Property(a => a.UserName)
            .HasMaxLength(256);

        builder.Property(a => a.Changes)
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(a => a.EntityName);

        builder.HasIndex(a => a.EntityId);

        builder.HasIndex(a => a.UserId);

        builder.HasIndex(a => a.CreatedDate);
    }
}