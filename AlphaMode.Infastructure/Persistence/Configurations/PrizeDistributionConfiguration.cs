using AlphaMode.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlphaMode.Infastructure.Persistence.Configurations
{
    public class PrizeDistributionConfiguration : IEntityTypeConfiguration<PrizeDistribution>
    {
        public void Configure(EntityTypeBuilder<PrizeDistribution> builder)
        {
            builder.ToTable("PrizeDistributions");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Position)
                .IsRequired();

            builder.Property(p => p.Amount)
                .HasPrecision(18, 2);

            builder.HasOne(p => p.PrizePool)
                .WithMany(p => p.Distributions)
                .HasForeignKey(p => p.PrizePoolId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(p => new
            {
                p.PrizePoolId,
                p.Position
            })
            .IsUnique();
        }
    }
}
