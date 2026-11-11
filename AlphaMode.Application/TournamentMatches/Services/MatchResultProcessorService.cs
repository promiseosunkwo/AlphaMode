using AlphaMode.Application.Common.Interfaces;
using AlphaMode.Application.Common.Models;
using AlphaMode.Application.TournamentMatches.Interfaces;
using AlphaMode.Domain.Entities;
using AlphaMode.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AlphaMode.Application.TournamentMatches.Services;

public class MatchResultProcessor : IMatchResultProcessor
{
    private readonly IApplicationDbContext _context;
    private readonly ITournamentStandingService _standingService;

    public MatchResultProcessor(IApplicationDbContext context, ITournamentStandingService standingService)
    {
        _context = context;
        _standingService = standingService;
    }

    public async Task<Result> ProcessAsync(
    Match match,
    int playerOneScore,
    int playerTwoScore,
    bool wentToExtraTime = false,
    int? playerOnePenaltyScore = null,
    int? playerTwoPenaltyScore = null,
    CancellationToken cancellationToken = default)
    {
        if (match.Tournament is null)
            return Result.Failure("Tournament could not be loaded.");

        if (match.PlayerOneId is null || match.PlayerTwoId is null || match.PlayerOne is null || match.PlayerTwo is null)
            return Result.Failure("Both players must be loaded.");

        var validation = MatchScoreValidator.Validate(
            match.Tournament.Format,
            playerOneScore,
            playerTwoScore,
            wentToExtraTime,
            playerOnePenaltyScore,
            playerTwoPenaltyScore);

        if (!validation.Succeeded)
            return validation;

        ApplyScore(
            match,
            playerOneScore,
            playerTwoScore,
            wentToExtraTime,
            playerOnePenaltyScore,
            playerTwoPenaltyScore);

        ApplyCareerStatistics(match, playerOneScore, playerTwoScore);

        if (match.Tournament.Format == TournamentFormat.League)
        {
            var standingResult = await _standingService.ApplyResultAsync(
                match,
                playerOneScore,
                playerTwoScore,
                cancellationToken);

            if (!standingResult.Succeeded)
                return standingResult;
        }

        match.Status = MatchStatus.Completed;

        return Result.Success();
    }



    private void ApplyScore(
    Match match,
    int playerOneScore,
    int playerTwoScore,
    bool wentToExtraTime,
    int? playerOnePenaltyScore,
    int? playerTwoPenaltyScore)
    {
        if (match.Score is null)
        {
            var score = new MatchScore
            {
                Id = Guid.NewGuid(),
                MatchId = match.Id
            };

            _context.MatchScores.Add(score);
            match.Score = score;
        }

        match.Score.PlayerOneScore = playerOneScore;
        match.Score.PlayerTwoScore = playerTwoScore;
        match.Score.WentToExtraTime = wentToExtraTime;
        match.Score.PlayerOnePenaltyScore = playerOnePenaltyScore;
        match.Score.PlayerTwoPenaltyScore = playerTwoPenaltyScore;

        if (playerOneScore > playerTwoScore)
        {
            match.WinnerId = match.PlayerOneId;
        }
        else if (playerTwoScore > playerOneScore)
        {
            match.WinnerId = match.PlayerTwoId;
        }
        else if (playerOnePenaltyScore.HasValue &&
                 playerTwoPenaltyScore.HasValue)
        {
            match.WinnerId =
                playerOnePenaltyScore.Value > playerTwoPenaltyScore.Value
                    ? match.PlayerOneId
                    : match.PlayerTwoId;
        }
        else
        {
            match.WinnerId = null;
        }
    }


    private static void ApplyCareerStatistics(Match match, int playerOneScore, int playerTwoScore)
    {
        if (playerOneScore > playerTwoScore)
        {
            match.PlayerOne!.Wins++;
            match.PlayerTwo!.Losses++;

            return;
        }

        if (playerTwoScore > playerOneScore)
        {
            match.PlayerTwo!.Wins++;
            match.PlayerOne!.Losses++;

            return;
        }

        match.PlayerOne!.Draws++;
        match.PlayerTwo!.Draws++;
    }

    public async Task<Result> ReverseAsync(
    Match match,
    CancellationToken cancellationToken = default)
    {
        if (match.Status != MatchStatus.Completed)
        {
            return Result.Failure(
                "Only a completed match result can be reversed.");
        }

        if (match.Score is null)
        {
            return Result.Failure(
                "The completed match does not have a score.");
        }

        if (match.PlayerOneId is null ||
            match.PlayerTwoId is null ||
            match.PlayerOne is null ||
            match.PlayerTwo is null)
        {
            return Result.Failure(
                "Both players are required to reverse this result.");
        }

        var playerOneScore = match.Score.PlayerOneScore;
        var playerTwoScore = match.Score.PlayerTwoScore;

        var playerOne = match.PlayerOne;
        var playerTwo = match.PlayerTwo;


        // =====================================================
        // REVERSE PLAYER CAREER STATISTICS
        // =====================================================

        if (playerOneScore > playerTwoScore)
        {
            if (playerOne.Wins <= 0 || playerTwo.Losses <= 0)
            {
                return Result.Failure(
                    "The existing player statistics are inconsistent " +
                    "with this match result.");
            }

            playerOne.Wins--;
            playerTwo.Losses--;
        }
        else if (playerTwoScore > playerOneScore)
        {
            if (playerTwo.Wins <= 0 || playerOne.Losses <= 0)
            {
                return Result.Failure(
                    "The existing player statistics are inconsistent " +
                    "with this match result.");
            }

            playerTwo.Wins--;
            playerOne.Losses--;
        }
        else
        {
            if (playerOne.Draws <= 0 || playerTwo.Draws <= 0)
            {
                return Result.Failure(
                    "The existing player statistics are inconsistent " +
                    "with this match result.");
            }

            playerOne.Draws--;
            playerTwo.Draws--;
        }


        // =====================================================
        // REVERSE LEAGUE STANDINGS
        // =====================================================

        if (match.Tournament.Format == TournamentFormat.League)
        {
            var reverseStandingResult =
                await ReverseLeagueStandingAsync(
                    match,
                    playerOneScore,
                    playerTwoScore,
                    cancellationToken);

            if (!reverseStandingResult.Succeeded)
            {
                return reverseStandingResult;
            }
        }


        // We deliberately do NOT remove MatchScore here.
        // CorrectResultAsync will overwrite it with the corrected score.

        match.WinnerId = null;

        return Result.Success();
    }

    private async Task<Result> ReverseLeagueStandingAsync(
    Match match,
    int playerOneScore,
    int playerTwoScore,
    CancellationToken cancellationToken)
    {
        if (match.PlayerOneId is null ||
            match.PlayerTwoId is null)
        {
            return Result.Failure(
                "Both players are required.");
        }

        var playerOneStanding =
            await _context.TournamentStandings
                .FirstOrDefaultAsync(
                    x =>
                        x.TournamentId == match.TournamentId &&
                        x.PlayerId == match.PlayerOneId.Value,
                    cancellationToken);

        var playerTwoStanding =
            await _context.TournamentStandings
                .FirstOrDefaultAsync(
                    x =>
                        x.TournamentId == match.TournamentId &&
                        x.PlayerId == match.PlayerTwoId.Value,
                    cancellationToken);

        if (playerOneStanding is null ||
            playerTwoStanding is null)
        {
            return Result.Failure(
                "The tournament standings for this match could not be found.");
        }


        // =====================================================
        // PLAYED
        // =====================================================

        if (playerOneStanding.Played <= 0 ||
            playerTwoStanding.Played <= 0)
        {
            return Result.Failure(
                "The tournament standings are inconsistent " +
                "with this match result.");
        }

        playerOneStanding.Played--;
        playerTwoStanding.Played--;


        // =====================================================
        // SCORES
        // =====================================================

        playerOneStanding.ScoreFor -= playerOneScore;
        playerOneStanding.ScoreAgainst -= playerTwoScore;

        playerTwoStanding.ScoreFor -= playerTwoScore;
        playerTwoStanding.ScoreAgainst -= playerOneScore;


        // =====================================================
        // RESULT
        // =====================================================

        if (playerOneScore > playerTwoScore)
        {
            if (playerOneStanding.Wins <= 0 ||
                playerTwoStanding.Losses <= 0)
            {
                return Result.Failure(
                    "The tournament standings are inconsistent " +
                    "with this match result.");
            }

            playerOneStanding.Wins--;
            playerTwoStanding.Losses--;

            playerOneStanding.Points -=
                match.Tournament.PointsForWin;

            playerTwoStanding.Points -=
                match.Tournament.PointsForLoss;
        }
        else if (playerTwoScore > playerOneScore)
        {
            if (playerTwoStanding.Wins <= 0 ||
                playerOneStanding.Losses <= 0)
            {
                return Result.Failure(
                    "The tournament standings are inconsistent " +
                    "with this match result.");
            }

            playerTwoStanding.Wins--;
            playerOneStanding.Losses--;

            playerTwoStanding.Points -=
                match.Tournament.PointsForWin;

            playerOneStanding.Points -=
                match.Tournament.PointsForLoss;
        }
        else
        {
            if (playerOneStanding.Draws <= 0 ||
                playerTwoStanding.Draws <= 0)
            {
                return Result.Failure(
                    "The tournament standings are inconsistent " +
                    "with this match result.");
            }

            playerOneStanding.Draws--;
            playerTwoStanding.Draws--;

            playerOneStanding.Points -=
                match.Tournament.PointsForDraw;

            playerTwoStanding.Points -=
                match.Tournament.PointsForDraw;
        }


        return Result.Success();
    }
}
