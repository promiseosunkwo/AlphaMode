using AlphaMode.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using RegistrationEntity = AlphaMode.Domain.Entities.TournamentRegistration;
namespace AlphaMode.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Player> Players { get; }

    DbSet<Game> Games { get; }

    DbSet<Tournament> Tournaments { get; }

    DbSet<RegistrationEntity> TournamentRegistrations { get; }

    DbSet<Match> Matches { get; }

    DbSet<MatchScore> MatchScores { get; }

    DbSet<Ranking> Rankings { get; }

    DbSet<PrizePool> PrizePools { get; }

    DbSet<PrizeDistribution> PrizeDistributions { get; }

    DbSet<Payment> Payments { get; }

    DbSet<Notification> Notifications { get; }

    DbSet<AuditLog> AuditLogs { get; }

    DbSet<TournamentStanding> TournamentStandings { get;}

    DbSet<FootballRatingHistory> FootballRatingHistories { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}