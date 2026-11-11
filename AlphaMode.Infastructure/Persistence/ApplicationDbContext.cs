using AlphaMode.Application.Common.Interfaces;
using AlphaMode.Domain.Common;
using AlphaMode.Domain.Entities;
using AlphaMode.Domain.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AlphaMode.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    #region DbSets

    public DbSet<Player> Players => Set<Player>();

    public DbSet<Game> Games => Set<Game>();

    public DbSet<Tournament> Tournaments => Set<Tournament>();

    public DbSet<TournamentRegistration> TournamentRegistrations => Set<TournamentRegistration>();

    public DbSet<Match> Matches => Set<Match>();

    public DbSet<MatchScore> MatchScores => Set<MatchScore>();

    public DbSet<Ranking> Rankings => Set<Ranking>();

    public DbSet<PrizePool> PrizePools => Set<PrizePool>();

    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<PrizeDistribution> PrizeDistributions => Set<PrizeDistribution>();

    public DbSet<TournamentStanding> TournamentStandings => Set<TournamentStanding>();

    public DbSet<FootballRatingHistory> FootballRatingHistories => Set<FootballRatingHistory>();


    #endregion

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly); // applies configration from my custom configuration classes in infastructure/persistence/configurations folder
    }


    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries().Where(e =>
                e.Entity is AuditableEntity &&
                (e.State == EntityState.Added ||
                 e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (AuditableEntity)entry.Entity;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedDate = DateTime.UtcNow;
            }

            entity.ModifiedDate = DateTime.UtcNow;
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}