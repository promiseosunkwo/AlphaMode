using AlphaMode.Application.Common.Interfaces;
using AlphaMode.Application.Common.Models;
using AlphaMode.Application.Tournaments.DTOs;
using AlphaMode.Application.Tournaments.Interfaces;
using AlphaMode.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AlphaMode.Application.Tournaments.Services;

public sealed class TournamentReadinessService : ITournamentReadinessService
{
    private readonly IApplicationDbContext _context;

    public TournamentReadinessService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<TournamentReadinessDto>> CheckAsync(Guid tournamentId, CancellationToken cancellationToken = default)
    {
        var tournament = await _context.Tournaments
            .AsNoTracking()
            .FirstOrDefaultAsync(
                t => t.Id == tournamentId,
                cancellationToken);

        if (tournament is null)
        {
            return Result<TournamentReadinessDto>.Failure("Tournament could not be found.");
        }

        var readiness = new TournamentReadinessDto
        {
            TournamentId = tournamentId
        };

        var now = DateTime.UtcNow;

        // Lifecycle
        if (tournament.Status != TournamentStatus.Upcoming)
        {
            readiness.Issues.Add("Tournament must be Upcoming before it can start.");
        }

        // Registration deadline
        if (tournament.RegistrationDeadline.HasValue &&
            now < tournament.RegistrationDeadline.Value)
        {
            readiness.Issues.Add(
                "Registration must close before the tournament starts.");
        }

        // Tournament dates
        if (tournament.StartDate >= tournament.EndDate)
        {
            readiness.Issues.Add(
                "Tournament end date must be after the start date.");
        }

        // Format
        if (tournament.Format == TournamentFormat.DoubleElimination)
        {
            readiness.Issues.Add(
                "Double-elimination tournaments are not supported yet.");
        }
        else if (tournament.Format is not
                 (TournamentFormat.League or
                  TournamentFormat.SingleElimination))
        {
            readiness.Issues.Add(
                "Tournament format is not supported.");
        }

        if (tournament.Legs < 1)
        {
            readiness.Issues.Add(
                "Tournament legs must be at least one.");
        }

        // Registered players
        readiness.RegisteredPlayers = await _context.TournamentRegistrations
                .AsNoTracking()
                .CountAsync(
                    r => r.TournamentId == tournamentId &&
                         (r.Status == TournamentRegistrationStatus.Registered ||
                          r.Status == TournamentRegistrationStatus.CheckedIn),
                    cancellationToken);

        // Eligible checked-in players
        var eligiblePlayerIds = await _context.TournamentRegistrations
                .AsNoTracking()
                .Where(r =>
                    r.TournamentId == tournamentId &&
                    r.Status == TournamentRegistrationStatus.CheckedIn &&
                    r.Player.IsActive)
                .Select(r => r.PlayerId)
                .ToListAsync(cancellationToken);

        readiness.CheckedInPlayers = eligiblePlayerIds.Count;

        if (eligiblePlayerIds.Count < 2)
        {
            readiness.Issues.Add("At least two active checked-in players are required.");
        }

        // Fixtures
        var fixtures = await _context.Matches
            .AsNoTracking()
            .Where(m => m.TournamentId == tournamentId)
            .Select(m => new
            {
                m.PlayerOneId,
                m.PlayerTwoId,
                m.RoundNumber
            })
            .ToListAsync(cancellationToken);

        readiness.FixturesCreated = fixtures.Count;

        if (fixtures.Count == 0)
        {
            readiness.Issues.Add(
                "Generate tournament fixtures before starting.");
        }
        else
        {
            var eligibleSet = eligiblePlayerIds.ToHashSet();

            var assignedPlayerIds = fixtures
                .SelectMany(m => new Guid?[]
                {
                    m.PlayerOneId,
                    m.PlayerTwoId
                })
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToHashSet();

            var ineligiblePlayers =
                assignedPlayerIds.Except(eligibleSet).ToList();

            if (ineligiblePlayers.Count > 0)
            {
                readiness.Issues.Add(
                    "Fixtures contain players who are no longer eligible.");
            }

            var missingPlayers = eligibleSet.Except(assignedPlayerIds).ToList();

            if (missingPlayers.Count > 0)
            {
                readiness.Issues.Add(
                    $"{missingPlayers.Count} checked-in player(s) are missing from fixtures.");
            }

            if (fixtures.Any(m => m.RoundNumber < 1))
            {
                readiness.Issues.Add(
                    "Some fixtures have invalid round numbers.");
            }

            if (tournament.Format == TournamentFormat.League &&
                fixtures.Any(m =>
                    !m.PlayerOneId.HasValue ||
                    !m.PlayerTwoId.HasValue))
            {
                readiness.Issues.Add(
                    "League fixtures must have two assigned players.");
            }
        }

        readiness.IsReady = readiness.Issues.Count == 0;

        return Result<TournamentReadinessDto>.Success(readiness);
    }
}