using AlphaMode.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PlayerConfiguration : IEntityTypeConfiguration<Player>
{
    public void Configure(EntityTypeBuilder<Player> builder)
    {
        builder.ToTable("Players");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.GamerTag)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.RankingPoints)
            .HasDefaultValue(0);

        builder.Property(x => x.Wins)
            .HasDefaultValue(0);

        builder.Property(x => x.Losses)
            .HasDefaultValue(0);

        builder.Property(x => x.Draws)
            .HasDefaultValue(0);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);


        builder.HasOne(x => x.User)
            .WithOne(x => x.Player)
            .HasForeignKey<Player>(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);


        builder.HasIndex(x => x.UserId)
            .IsUnique();


        builder.HasIndex(x => x.GamerTag)
            .IsUnique();
    }
}