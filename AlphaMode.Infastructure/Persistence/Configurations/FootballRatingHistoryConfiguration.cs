using AlphaMode.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphaMode.Infastructure.Persistence.Configurations;

public class FootballRatingHistoryConfiguration : IEntityTypeConfiguration<FootballRatingHistory>
{
    public void Configure(EntityTypeBuilder<FootballRatingHistory> builder)
    {
        builder.HasIndex(x => new
        {
            x.MatchId,
            x.PlayerId
        }).IsUnique();

        builder.HasIndex(x => x.ProcessingSequence);

        builder.Property(x => x.RatingBefore)
            .HasPrecision(12, 4);

        builder.Property(x => x.RatingChange)
            .HasPrecision(12, 4);

        builder.Property(x => x.RatingAfter)
            .HasPrecision(12, 4);

        builder.Property(x => x.ResultScore)
            .HasPrecision(3, 1);

        builder.HasOne(x => x.Match)
            .WithMany()
            .HasForeignKey(x => x.MatchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Player)
            .WithMany()
            .HasForeignKey(x => x.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.OpponentPlayer)
            .WithMany()
            .HasForeignKey(x => x.OpponentPlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Game)
            .WithMany()
            .HasForeignKey(x => x.GameId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
