using AlphaMode.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class TournamentConfiguration : IEntityTypeConfiguration<Tournament>
{
    public void Configure(EntityTypeBuilder<Tournament> builder)
    {
        builder.ToTable("Tournaments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.EntryFee)
            .HasColumnType("decimal(18,2)");

        builder.Property(t => t.Slug)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasIndex(t => t.Slug)
            .IsUnique();

        builder.Property(t => t.ImageUrl)
            .HasMaxLength(500);


        builder.Property(t => t.MaxPlayers)
        .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.HasOne(x => x.Game)
            .WithMany(x => x.Tournaments)
            .HasForeignKey(x => x.GameId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Registrations)
            .WithOne(x => x.Tournament)
            .HasForeignKey(x => x.TournamentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Matches)
            .WithOne(x => x.Tournament)
            .HasForeignKey(x => x.TournamentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => t.GameId);

        builder.HasIndex(t => t.Status);

        builder.HasIndex(t => t.StartDate);
    }
}