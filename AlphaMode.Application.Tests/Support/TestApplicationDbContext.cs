using AlphaMode.Application.Common.Interfaces;
using AlphaMode.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AlphaMode.Application.Tests.Support;

internal sealed class TestApplicationDbContext : DbContext, IApplicationDbContext
{
    private long _sequence;
    public TestApplicationDbContext(string? databaseName = null) : base(new DbContextOptionsBuilder<TestApplicationDbContext>().UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString()).Options) { }
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Game> Games => Set<Game>();
    public DbSet<Tournament> Tournaments => Set<Tournament>();
    // Fully qualify the entity because the test project's TournamentRegistration namespace
    // would otherwise be selected by the compiler instead of the domain model.
    public DbSet<AlphaMode.Domain.Entities.TournamentRegistration> TournamentRegistrations => Set<AlphaMode.Domain.Entities.TournamentRegistration>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<MatchScore> MatchScores => Set<MatchScore>();
    public DbSet<Ranking> Rankings => Set<Ranking>();
    public DbSet<PrizePool> PrizePools => Set<PrizePool>();
    public DbSet<PrizeDistribution> PrizeDistributions => Set<PrizeDistribution>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<TournamentStanding> TournamentStandings => Set<TournamentStanding>();
    public DbSet<FootballRatingHistory> FootballRatingHistories => Set<FootballRatingHistory>();
    public Task<long> GetNextFootballRatingSequenceAsync(CancellationToken cancellationToken = default) => Task.FromResult(Interlocked.Increment(ref _sequence));
}
