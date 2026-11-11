using AlphaMode.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlphaMode.Infastructure.Persistence.Configurations
{
    public class PrizePoolConfiguration : IEntityTypeConfiguration<PrizePool>
    {
        public void Configure(EntityTypeBuilder<PrizePool> builder)
        {
            builder.ToTable("PrizePools");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.TotalPrize)
                .HasPrecision(18, 2);

            builder.Property(p => p.Currency)
                .IsRequired()
                .HasMaxLength(3);

            builder.HasOne(p => p.Tournament)
                .WithOne()
                .HasForeignKey<PrizePool>(p => p.TournamentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(p => p.TournamentId)
                .IsUnique();

            builder.HasMany(x => x.Distributions)
            .WithOne(x => x.PrizePool)
            .HasForeignKey(x => x.PrizePoolId)
            .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
