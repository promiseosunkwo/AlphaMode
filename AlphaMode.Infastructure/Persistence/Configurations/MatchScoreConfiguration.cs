using AlphaMode.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class MatchScoreConfiguration : IEntityTypeConfiguration<MatchScore>
{
    public void Configure(EntityTypeBuilder<MatchScore> builder)
    {
        builder.ToTable("MatchScores");

        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Match)
            .WithOne(x => x.Score)
            .HasForeignKey<MatchScore>(x => x.MatchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => s.MatchId)
        .IsUnique();

        builder.Property(s => s.PlayerOneScore)
            .HasDefaultValue(0);

        builder.Property(s => s.PlayerTwoScore)
            .HasDefaultValue(0);
    }
}