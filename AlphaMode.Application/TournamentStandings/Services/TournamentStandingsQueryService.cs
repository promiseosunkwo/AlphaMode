using AlphaMode.Application.Common.Interfaces;
using AlphaMode.Application.Common.Models;
using AlphaMode.Application.TournamentStandings.DTOs;
using AlphaMode.Application.TournamentStandings.Interfaces;
using AlphaMode.Domain.Entities;
using AlphaMode.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AlphaMode.Application.TournamentStandings.Services;

public class TournamentStandingsQueryService : ITournamentStandingsQueryService
{
    private readonly IApplicationDbContext _context;

    public TournamentStandingsQueryService(IApplicationDbContext context)
    {
        _context = context;
    }


    public async Task<Result> InitialiseAsync(Guid tournamentId, IReadOnlyCollection<Guid> playerIds, CancellationToken cancellationToken = default)
    {
        if (playerIds.Count == 0)
        {
            return Result.Failure(
                "There are no eligible players to initialise standings for.");
        }

        var existingPlayerIds = await _context.TournamentStandings
            .Where(s =>
                s.TournamentId == tournamentId &&
                playerIds.Contains(s.PlayerId))
            .Select(s => s.PlayerId)
            .ToListAsync(cancellationToken);

        var existingPlayerIdSet = existingPlayerIds.ToHashSet();

        var missingPlayerIds = playerIds
            .Where(playerId => !existingPlayerIdSet.Contains(playerId))
            .Distinct()
            .ToList();

        foreach (var playerId in missingPlayerIds)
        {
            var standing = new TournamentStanding
            {
                TournamentId = tournamentId,
                PlayerId = playerId,
                Played = 0,
                Wins = 0,
                Draws = 0,
                Losses = 0,
                ScoreFor = 0,
                ScoreAgainst = 0,
                Points = 0
            };

            _context.TournamentStandings.Add(standing);
        }

        return Result.Success();
    }

    public async Task<Result<LeagueTournamentStandingsDto>> GetAsync(Guid tournamentId, CancellationToken cancellationToken = default)
    {
        if (tournamentId == Guid.Empty)
        {
            return Result<LeagueTournamentStandingsDto>
                .Failure("Tournament is required.");
        }

        var tournament = await _context.Tournaments
            .AsNoTracking()
            .Where(x => x.Id == tournamentId)
            .Select(x => new
            {
                x.Id,
                x.Name,
                GameName = x.Game.Name,
                x.Format
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (tournament is null)
        {
            return Result<LeagueTournamentStandingsDto>
                .Failure("Tournament was not found.");
        }

        if (tournament.Format != TournamentFormat.League)
        {
            return Result<LeagueTournamentStandingsDto>
                .Failure(
                    "Standings are only available for league tournaments.");
        }

        var standings = await _context.TournamentStandings
            .AsNoTracking()
            .Where(x => x.TournamentId == tournamentId)
            .OrderByDescending(x => x.Points)
            .ThenByDescending(x => x.ScoreFor - x.ScoreAgainst)
            .ThenByDescending(x => x.ScoreFor)
            .ThenByDescending(x => x.Wins)
            .ThenBy(x => x.Player.GamerTag)
            .Select(x => new LeagueTournamentStandingDto
            {
                PlayerId = x.PlayerId,
                GamerTag = x.Player.GamerTag,

                Played = x.Played,
                Wins = x.Wins,
                Draws = x.Draws,
                Losses = x.Losses,

                ScoreFor = x.ScoreFor,
                ScoreAgainst = x.ScoreAgainst,

                ScoreDifference =
                    x.ScoreFor - x.ScoreAgainst,

                Points = x.Points
            })
            .ToListAsync(cancellationToken);

        for (var index = 0; index < standings.Count; index++)
        {
            standings[index].Position = index + 1;
        }

        return Result<LeagueTournamentStandingsDto>.Success(
            new LeagueTournamentStandingsDto
            {
                TournamentId = tournament.Id,
                TournamentName = tournament.Name,
                GameName = tournament.GameName,
                Format = tournament.Format,
                Standings = standings
            });
    }
}