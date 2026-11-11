using AlphaMode.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Amount)
            .HasPrecision(18, 2);

        builder.Property(p => p.TransactionReference)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(p => p.TransactionReference)
            .IsUnique();

        builder.Property(p => p.ProviderReference)
            .HasMaxLength(150);

        builder.HasOne(p => p.Player)
            .WithMany()
            .HasForeignKey(p => p.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Tournament)
            .WithMany()
            .HasForeignKey(p => p.TournamentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(p => p.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.HasIndex(p => p.PlayerId);

        builder.HasIndex(p => p.TournamentId);

        builder.HasIndex(p => p.Status);

        builder.HasOne(x => x.TournamentRegistration)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.TournamentRegistrationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}