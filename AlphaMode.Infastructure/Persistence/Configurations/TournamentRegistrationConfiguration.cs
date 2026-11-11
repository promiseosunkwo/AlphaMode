using AlphaMode.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class TournamentRegistrationConfiguration
    : IEntityTypeConfiguration<TournamentRegistration>
{
    public void Configure(
        EntityTypeBuilder<TournamentRegistration> builder)
    {
        builder.ToTable("TournamentRegistrations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.DisqualificationReason)
            .HasMaxLength(500);

        builder.HasOne(x => x.Tournament)
            .WithMany(x => x.Registrations)
            .HasForeignKey(x => x.TournamentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Player)
            .WithMany(x => x.Registrations)
            .HasForeignKey(x => x.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x =>
                new
                {
                    x.TournamentId,
                    x.PlayerId
                })
            .IsUnique();

        builder.HasIndex(x => x.Status);

        builder.HasIndex(x => x.TournamentId);
    }
}