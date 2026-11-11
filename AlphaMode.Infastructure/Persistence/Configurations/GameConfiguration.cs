using AlphaMode.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class GameConfiguration : IEntityTypeConfiguration<Game>
{
    public void Configure(EntityTypeBuilder<Game> builder)
    {
        builder.ToTable("Games");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Version)
            .HasMaxLength(50);

        builder.Property(x => x.Platform)
            .HasMaxLength(50);

        builder.Property(g => g.ImageUrl)
        .HasMaxLength(500);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.Property(g => g.Slug)
        .IsRequired()
        .HasMaxLength(100);

        builder.HasIndex(g => g.Slug)
            .IsUnique();

        builder.HasMany(g => g.Tournaments)
        .WithOne(t => t.Game)
        .HasForeignKey(t => t.GameId)
        .OnDelete(DeleteBehavior.Restrict);
    }
}