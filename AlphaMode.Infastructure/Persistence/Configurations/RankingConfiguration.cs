using AlphaMode.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RankingConfiguration : IEntityTypeConfiguration<Ranking>
{
    public void Configure(EntityTypeBuilder<Ranking> builder)
    {
        builder.ToTable("Rankings");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Points)
            .HasDefaultValue(0);


        builder.HasOne(r => r.Player)
            .WithMany()
            .HasForeignKey(r => r.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Game)
            .WithMany()
            .HasForeignKey(r => r.GameId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => new
        {
            r.PlayerId,
            r.GameId
        })
        .IsUnique();

        builder.HasIndex(r => new
        {
            r.GameId
        });

        builder.Property(x => x.EloRating)
           .HasPrecision(12, 4)
           .HasDefaultValue(1000m);

        builder.HasIndex(x => new
        {
            x.GameId,
            x.EloRating
        });

        builder.Ignore(x => x.GoalDifference);

        builder.Ignore(x => x.WinRate);
    }
}