
using AlphaMode.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlphaMode.Infastructure.Persistence.Configurations
{
    public class TournamentStandingConfiguration : IEntityTypeConfiguration<TournamentStanding>
    {
        public void Configure(EntityTypeBuilder<TournamentStanding> builder)
        {
            builder.HasIndex(s => new
            {
                s.TournamentId,
                s.PlayerId
            })
            .IsUnique();

            builder.HasOne(s => s.Tournament)
                .WithMany(t => t.Standings)
                .HasForeignKey(s => s.TournamentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(s => s.Player)
                .WithMany()
                .HasForeignKey(s => s.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Ignore(s => s.ScoreDifference);
        }
    }
}
