using AlphaMode.Domain.Entities;
using AlphaMode.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.ToTable("Matches");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.RoundNumber)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        // Match result type: Played, Walkover or DoubleForfeit
        builder.Property(x => x.ResultType)
            .HasConversion<int>()
            .HasDefaultValue(MatchResultType.Played)
            .IsRequired();

        builder.Property(x => x.ForfeitReason)
            .HasConversion<int?>();

        builder.Property(x => x.ForfeitingPlayerId)
            .IsRequired(false);

        builder.Property(x => x.ForfeitNotes)
            .HasMaxLength(1000);

        builder.Property(x => x.ForfeitedAt)
            .IsRequired(false);


        builder.HasOne(x => x.Tournament)
            .WithMany(x => x.Matches)
            .HasForeignKey(x => x.TournamentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.PlayerOne)
            .WithMany()
            .HasForeignKey(x => x.PlayerOneId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PlayerTwo)
            .WithMany()
            .HasForeignKey(x => x.PlayerTwoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Winner)
            .WithMany()
            .HasForeignKey(x => x.WinnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Score)
            .WithOne(s => s.Match)
            .HasForeignKey<MatchScore>(s => s.MatchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}