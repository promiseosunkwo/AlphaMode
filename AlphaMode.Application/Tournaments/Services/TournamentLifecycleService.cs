using AlphaMode.Application.Common.Interfaces;
using AlphaMode.Application.Common.Models;
using AlphaMode.Application.Tournaments.Interfaces;
using AlphaMode.Domain.Entities;
using AlphaMode.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AlphaMode.Application.Tournaments.Services;

public sealed class TournamentLifecycleService : ITournamentLifecycleService
{
    private readonly IApplicationDbContext _context;

    private readonly ITournamentReadinessService _readinessService;


    public TournamentLifecycleService( IApplicationDbContext context, ITournamentReadinessService readinessService)
    {
        _context = context;
        _readinessService = readinessService;
    }

    public async Task<Result> PublishAsync(Guid tournamentId, CancellationToken cancellationToken = default)
    {
        var tournament = await _context.Tournaments.FirstOrDefaultAsync(x => x.Id == tournamentId, cancellationToken);

        if (tournament is null)
            return Result.Failure("Tournament could not be found.");

        if (tournament.Status != TournamentStatus.Draft)
            return Result.Failure(
                "Only draft tournaments can be published.");

        if (tournament.StartDate <= DateTime.UtcNow)
            return Result.Failure(
                "The tournament start date must be in the future before it can be published.");

        if (tournament.RegistrationOpenDate.HasValue &&
            tournament.RegistrationDeadline.HasValue &&
            tournament.RegistrationOpenDate.Value >=
            tournament.RegistrationDeadline.Value)
        {
            return Result.Failure(
                "Registration must open before the registration deadline.");
        }

        if (tournament.RegistrationDeadline.HasValue && tournament.RegistrationDeadline.Value >
            tournament.StartDate)
        {
            return Result.Failure(
                "Registration must close before the tournament starts.");
        }

        tournament.Status = DeterminePublishedStatus(tournament);

        await _context.SaveChangesAsync(
            cancellationToken);

        return Result.Success(
            tournament.Status ==
            TournamentStatus.RegistrationOpen
                ? "Tournament published and registration is now open."
                : "Tournament published successfully.");
    }




    public async Task<Result> StartAsync(Guid tournamentId, CancellationToken cancellationToken = default)
    {
        var syncResult = await SynchroniseStatusAsync(tournamentId, cancellationToken);

        if (!syncResult.Succeeded)
        {
            return Result.Failure(syncResult.Error ?? "Tournament status could not be synchronised.");
        }

        var tournament = await _context.Tournaments.FirstOrDefaultAsync(t => t.Id == tournamentId, cancellationToken);

        if (tournament is null)
            return Result.Failure("Tournament could not be found.");

         // let's use this if the background service causes issues because it only starts with upcoming. Include this in the if statement - && tournament.Status != TournamentStatus.RegistrationOpen
        if (tournament.Status != TournamentStatus.Upcoming)
        {
            return Result.Failure($"The tournament cannot be started from its current status: {tournament.Status}.");
        }

        var now = DateTime.UtcNow;

        if (tournament.RegistrationDeadline.HasValue &&
            now < tournament.RegistrationDeadline.Value)
        {
            return Result.Failure(
                "Registration must close before the tournament starts.");
        }

        if (tournament.StartDate >= tournament.EndDate)
        {
            return Result.Failure(
                "Tournament start and end dates are invalid.");
        }

        if (tournament.Format == TournamentFormat.DoubleElimination)
        {
            return Result.Failure(
                "Double-elimination tournaments are not supported yet.");
        }

        // Get active, checked-in players.
        var eligiblePlayerIds = await _context.TournamentRegistrations
            .AsNoTracking()
            .Where(r =>
                r.TournamentId == tournamentId &&
                r.Status == TournamentRegistrationStatus.CheckedIn &&
                r.Player.IsActive)
            .Select(r => r.PlayerId)
            .ToListAsync(cancellationToken);

        if (eligiblePlayerIds.Count < 2)
        {
            return Result.Failure(
                "At least two checked-in players are required.");
        }

        // Load existing fixtures.
        var matches = await _context.Matches
            .AsNoTracking()
            .Where(m => m.TournamentId == tournamentId)
            .ToListAsync(cancellationToken);

        if (matches.Count == 0)
        {
            return Result.Failure(
                "Generate tournament fixtures before starting.");
        }

        var eligibleSet = eligiblePlayerIds.ToHashSet();

        // Collect players assigned to fixtures.
        var fixturePlayerIds = matches
            .SelectMany(m => new Guid?[]
            {
            m.PlayerOneId,
            m.PlayerTwoId
            })
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToHashSet();

        // Ensure fixtures don't contain ineligible players.
        if (fixturePlayerIds.Any(id => !eligibleSet.Contains(id)))
        {
            return Result.Failure(
                "Fixtures contain players who are no longer eligible.");
        }

        // Ensure every eligible player appears in the fixtures.
        if (eligibleSet.Any(id => !fixturePlayerIds.Contains(id)))
        {
            return Result.Failure(
                "Some checked-in players are missing from the fixtures.");
        }

        // League fixtures must have both participants.
        if (tournament.Format == TournamentFormat.League &&
            matches.Any(m =>
                !m.PlayerOneId.HasValue ||
                !m.PlayerTwoId.HasValue))
        {
            return Result.Failure(
                "Some league fixtures have missing players.");
        }

        tournament.Status = TournamentStatus.Live;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(
            "Tournament started successfully.");
    }



    public async Task<Result> CompleteAsync(Guid tournamentId, CancellationToken cancellationToken = default)
    {
        var tournament = await _context.Tournaments.FirstOrDefaultAsync(x => x.Id == tournamentId, cancellationToken);

        if (tournament is null)
            return Result.Failure("Tournament could not be found.");

        if (tournament.Status != TournamentStatus.Live)
            return Result.Failure("Only a live tournament can be completed.");

        var hasMatches = await _context.Matches.AnyAsync(x => x.TournamentId == tournamentId, cancellationToken);

        if (!hasMatches)
            return Result.Failure("The tournament does not have any matches.");

        var unfinishedMatches = await _context.Matches.AnyAsync(x =>
                        x.TournamentId == tournamentId &&
                        x.Status != MatchStatus.Completed, cancellationToken);

        if (unfinishedMatches)
            return Result.Failure("The tournament cannot be completed while matches are still unfinished.");

        tournament.Status = TournamentStatus.Completed;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success("Tournament completed successfully.");
    }



    public async Task<Result> CancelAsync(Guid tournamentId, CancellationToken cancellationToken = default)
    {
        var tournament = await _context.Tournaments.FirstOrDefaultAsync(x => x.Id == tournamentId, cancellationToken);

        if (tournament is null) return Result.Failure("Tournament could not be found.");

        if (tournament.Status == TournamentStatus.Cancelled)
        {
            return Result.Failure("The tournament has already been cancelled.");
        }

        if (tournament.Status == TournamentStatus.Completed)
        {
            return Result.Failure("A completed tournament cannot be cancelled.");
        }

        if (tournament.Status == TournamentStatus.Live)
        {
            return Result.Failure("A live tournament cannot be cancelled through the standard cancellation process.");
        }

        tournament.Status = TournamentStatus.Cancelled;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success("Tournament cancelled successfully.");
    }




    public async Task<Result<TournamentStatus>>SynchroniseStatusAsync(Guid tournamentId, CancellationToken cancellationToken = default)
    {
        var tournament = await _context.Tournaments.FirstOrDefaultAsync(x => x.Id == tournamentId, cancellationToken);

        if (tournament is null)
        {
            return Result<TournamentStatus>.Failure("Tournament could not be found.");
        }

        if (tournament.Status is
            TournamentStatus.Draft or
            TournamentStatus.Live or
            TournamentStatus.Completed or
            TournamentStatus.Cancelled)
        {
            return Result<TournamentStatus>.Success(tournament.Status);
        }

        var expectedStatus = DeterminePublishedStatus(tournament);

        if (tournament.Status == expectedStatus)
        {
            return Result<TournamentStatus>.Success(tournament.Status);
        }

        tournament.Status = expectedStatus;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<TournamentStatus>.Success(tournament.Status);
    }




    private static TournamentStatus DeterminePublishedStatus(Tournament tournament)
    {
        var now = DateTime.UtcNow;

        // Registration cannot remain open after the tournament starts.
        if (now >= tournament.StartDate)
        {
            return TournamentStatus.Upcoming;
        }

        // Registration has closed.
        if (tournament.RegistrationDeadline.HasValue && now >= tournament.RegistrationDeadline.Value)
        {
            return TournamentStatus.Upcoming;
        }

        // Registration has not opened yet.
        if (tournament.RegistrationOpenDate.HasValue && now < tournament.RegistrationOpenDate.Value)
        {
            return TournamentStatus.Upcoming;
        }

        // Registration is within its permitted window.
        return TournamentStatus.RegistrationOpen;
    }
}