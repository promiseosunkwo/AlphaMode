using AlphaMode.Application.Common.Interfaces;
using AlphaMode.Application.Common.Models;
using AlphaMode.Application.TournamentMatches.Interfaces;
using AlphaMode.Domain.Entities;
using AlphaMode.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AlphaMode.Application.TournamentMatches.Services;

public class AlphaModeStandingService : ITournamentStandingService
{
    private readonly IApplicationDbContext _context;

    public AlphaModeStandingService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> ApplyResultAsync(Match match, int playerOneScore, int playerTwoScore, CancellationToken cancellationToken = default)
    {
        if (match.PlayerOneId is null || match.PlayerTwoId is null)
        {
            return Result.Failure("Both players must be assigned before standings can be updated.");
        }

        var playerOneStanding = await GetOrCreateStandingAsync(
            match.TournamentId,
            match.PlayerOneId.Value,
            cancellationToken);

        var playerTwoStanding = await GetOrCreateStandingAsync(
            match.TournamentId,
            match.PlayerTwoId.Value,
            cancellationToken);

        playerOneStanding.Played++;
        playerTwoStanding.Played++;

        playerOneStanding.ScoreFor += playerOneScore;
        playerOneStanding.ScoreAgainst += playerTwoScore;

        playerTwoStanding.ScoreFor += playerTwoScore;
        playerTwoStanding.ScoreAgainst += playerOneScore;

        if (playerOneScore > playerTwoScore)
        {
            ApplyPlayerOneWin(match, playerOneStanding, playerTwoStanding);
        }
        else if (playerTwoScore > playerOneScore)
        {
            ApplyPlayerTwoWin(match, playerOneStanding, playerTwoStanding);
        }
        else
        {
            ApplyDraw(match, playerOneStanding, playerTwoStanding);
        }
        return Result.Success();
    }




    public async Task<Result> ApplyForfeitAsync(
        Match match,
        CancellationToken cancellationToken = default)
    {
        if (match.Tournament.Format != TournamentFormat.League)
        {
            return Result.Failure(
                "Forfeiture standings can only be updated for league tournaments.");
        }

        if (match.PlayerOneId is null || match.PlayerTwoId is null)
        {
            return Result.Failure(
                "Both players must be assigned before standings can be updated.");
        }

        if (match.ResultType != MatchResultType.Walkover &&
            match.ResultType != MatchResultType.DoubleForfeit)
        {
            return Result.Failure(
                "The match does not have a valid forfeiture result.");
        }

        if (match.ResultType == MatchResultType.Walkover &&
            (match.ForfeitingPlayerId is null ||
             (match.ForfeitingPlayerId != match.PlayerOneId &&
              match.ForfeitingPlayerId != match.PlayerTwoId)))
        {
            return Result.Failure(
                "A valid forfeiting player is required.");
        }

        var playerOneStanding = await GetOrCreateStandingAsync(
            match.TournamentId,
            match.PlayerOneId.Value,
            cancellationToken);

        var playerTwoStanding = await GetOrCreateStandingAsync(
            match.TournamentId,
            match.PlayerTwoId.Value,
            cancellationToken);

        playerOneStanding.Played++;
        playerTwoStanding.Played++;

        if (match.ResultType == MatchResultType.DoubleForfeit)
        {
            playerOneStanding.Losses++;
            playerTwoStanding.Losses++;

            // Both players receive zero points.
            // No goals are recorded.
            return Result.Success();
        }

        if (match.ForfeitingPlayerId == match.PlayerOneId)
        {
            playerOneStanding.Losses++;
            playerTwoStanding.Wins++;

            playerOneStanding.Points += match.Tournament.PointsForLoss;
            playerTwoStanding.Points += match.Tournament.PointsForWin;
        }
        else
        {
            playerTwoStanding.Losses++;
            playerOneStanding.Wins++;

            playerTwoStanding.Points += match.Tournament.PointsForLoss;
            playerOneStanding.Points += match.Tournament.PointsForWin;
        }

        return Result.Success();
    }




    private async Task<TournamentStanding> GetOrCreateStandingAsync(Guid tournamentId, Guid playerId, CancellationToken cancellationToken)
    {
        var standing = await _context.TournamentStandings.FirstOrDefaultAsync(
                x => x.TournamentId == tournamentId &&
                     x.PlayerId == playerId,
                cancellationToken);

        if (standing is not null)
        {
            return standing;
        }

        standing = new TournamentStanding
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

        return standing;
    }


    private static void ApplyPlayerOneWin(
        Match match,
        TournamentStanding playerOneStanding,
        TournamentStanding playerTwoStanding)
    {
        playerOneStanding.Wins++;
        playerTwoStanding.Losses++;

        playerOneStanding.Points += match.Tournament.PointsForWin;
        playerTwoStanding.Points += match.Tournament.PointsForLoss;
    }


    private static void ApplyPlayerTwoWin(
        Match match,
        TournamentStanding playerOneStanding,
        TournamentStanding playerTwoStanding)
    {
        playerTwoStanding.Wins++;
        playerOneStanding.Losses++;

        playerTwoStanding.Points += match.Tournament.PointsForWin;
        playerOneStanding.Points += match.Tournament.PointsForLoss;
    }



    private static void ApplyDraw(
        Match match,
        TournamentStanding playerOneStanding,
        TournamentStanding playerTwoStanding)
    {
        playerOneStanding.Draws++;
        playerTwoStanding.Draws++;

        playerOneStanding.Points += match.Tournament.PointsForDraw;
        playerTwoStanding.Points += match.Tournament.PointsForDraw;
    }
}
