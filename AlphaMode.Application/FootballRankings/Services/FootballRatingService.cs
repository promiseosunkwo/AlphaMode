using AlphaMode.Application.Common.Interfaces;
using AlphaMode.Application.Common.Models;
using AlphaMode.Application.FootballRankings.Interfaces;
using AlphaMode.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AlphaMode.Application.FootballRankings.Services;

public sealed class FootballRatingService : IFootballRatingService
{
    private readonly IApplicationDbContext _context;

    public FootballRatingService(IApplicationDbContext context) => _context = context;

    public async Task<Result> ApplyMatchAsync(Match match, CancellationToken cancellationToken = default)
    {
        var eligibility = RankedMatchEligibilityPolicy.Validate(match);
        // Ineligible results are valid match outcomes; they simply have no rating side effect.
        if (!eligibility.Succeeded) return Result.Success(eligibility.Error);

        var existingRows = await _context.FootballRatingHistories.AsNoTracking()
            .CountAsync(x => x.MatchId == match.Id, cancellationToken);
        if (existingRows == 2) return Result.Success("Elo was already processed for this match.");
        if (existingRows != 0) return Result.Failure("The match has incomplete Elo history and requires a rebuild.");

        var sequence = await _context.GetNextFootballRatingSequenceAsync(cancellationToken);
        await ApplyCoreAsync(match, sequence, cancellationToken);
        return Result.Success("Elo ratings updated.");
    }

    public async Task<Result> RebuildGameAsync(Guid gameId, CancellationToken cancellationToken = default)
    {
        if (gameId == Guid.Empty) return Result.Failure("Game is required for an Elo rebuild.");

        // Preserve the original match order because Elo is order-dependent.
        var processedMatches = await _context.FootballRatingHistories.AsNoTracking()
            .Where(x => x.GameId == gameId)
            .GroupBy(x => x.MatchId)
            .Select(x => new { MatchId = x.Key, Sequence = x.Min(y => y.ProcessingSequence) })
            .OrderBy(x => x.Sequence)
            .ToListAsync(cancellationToken);

        // Load and remove through the abstraction instead of using provider-specific bulk SQL.
        // The surrounding transaction ensures deletes and rebuilt rows commit as one operation.
        var oldHistory = await _context.FootballRatingHistories
            .Where(x => x.GameId == gameId)
            .ToListAsync(cancellationToken);
        _context.FootballRatingHistories.RemoveRange(oldHistory);

        var rankings = await _context.Rankings.Where(x => x.GameId == gameId).ToListAsync(cancellationToken);
        foreach (var ranking in rankings)
        {
            // Rebuild from the ledger rather than applying compensating changes, which would leave
            // every later match calculated from an incorrect starting rating.
            ranking.EloRating = EloCalculator.InitialRating;
            ranking.MatchesPlayed = 0;
            ranking.Wins = 0;
            ranking.Draws = 0;
            ranking.Losses = 0;
            ranking.GoalsFor = 0;
            ranking.GoalsAgainst = 0;
            ranking.LastRankedMatchAt = null;
        }

        foreach (var item in processedMatches)
        {
            var match = await _context.Matches
                .Include(x => x.Score)
                .Include(x => x.PlayerOne)
                .Include(x => x.PlayerTwo)
                .FirstOrDefaultAsync(x => x.Id == item.MatchId, cancellationToken);

            if (match is null) return Result.Failure($"Ranked match {item.MatchId} could not be loaded for replay.");
            var eligibility = RankedMatchEligibilityPolicy.Validate(match);
            if (!eligibility.Succeeded) continue;
            await ApplyCoreAsync(match, item.Sequence, cancellationToken);
        }

        return Result.Success("Game Elo ratings rebuilt successfully.");
    }

    private async Task ApplyCoreAsync(Match match, long sequence, CancellationToken cancellationToken)
    {
        var playerOneId = match.PlayerOneId!.Value;
        var playerTwoId = match.PlayerTwoId!.Value;
        var score = match.Score!;

        var playerOneRanking = await GetOrCreateRankingAsync(playerOneId, match.GameId, cancellationToken);
        var playerTwoRanking = await GetOrCreateRankingAsync(playerTwoId, match.GameId, cancellationToken);

        decimal playerOneResult = match.WinnerId == playerOneId ? 1m : match.WinnerId == playerTwoId ? 0m : 0.5m;
        var calculation = EloCalculator.Calculate(playerOneRanking.EloRating, playerTwoRanking.EloRating, playerOneResult);

        ApplyRankingResult(playerOneRanking, calculation.PlayerOneRatingAfter, playerOneResult, score.PlayerOneScore, score.PlayerTwoScore, match.ScheduledAt);
        ApplyRankingResult(playerTwoRanking, calculation.PlayerTwoRatingAfter, calculation.PlayerTwoResult, score.PlayerTwoScore, score.PlayerOneScore, match.ScheduledAt);

        var processedAt = DateTime.UtcNow;
        _context.FootballRatingHistories.AddRange(
            CreateHistory(match, playerOneId, playerTwoId, calculation.PlayerOneRatingBefore, calculation.PlayerOneChange, calculation.PlayerOneRatingAfter, score.PlayerOneScore, score.PlayerTwoScore, calculation.PlayerOneResult, sequence, processedAt),
            CreateHistory(match, playerTwoId, playerOneId, calculation.PlayerTwoRatingBefore, calculation.PlayerTwoChange, calculation.PlayerTwoRatingAfter, score.PlayerTwoScore, score.PlayerOneScore, calculation.PlayerTwoResult, sequence, processedAt));
    }

    private async Task<Ranking> GetOrCreateRankingAsync(Guid playerId, Guid gameId, CancellationToken cancellationToken)
    {
        var ranking = await _context.Rankings.FirstOrDefaultAsync(x => x.PlayerId == playerId && x.GameId == gameId, cancellationToken);
        if (ranking is not null) return ranking;

        ranking = new Ranking { Id = Guid.NewGuid(), PlayerId = playerId, GameId = gameId, EloRating = EloCalculator.InitialRating };
        _context.Rankings.Add(ranking);
        return ranking;
    }

    private static void ApplyRankingResult(Ranking ranking, decimal ratingAfter, decimal result, int goalsFor, int goalsAgainst, DateTime? playedAt)
    {
        ranking.EloRating = ratingAfter;
        ranking.MatchesPlayed++;
        if (result == 1m) ranking.Wins++;
        else if (result == 0.5m) ranking.Draws++;
        else ranking.Losses++;
        ranking.GoalsFor += goalsFor;
        ranking.GoalsAgainst += goalsAgainst;
        ranking.LastRankedMatchAt = playedAt ?? DateTime.UtcNow;
    }

    private static FootballRatingHistory CreateHistory(Match match, Guid playerId, Guid opponentId, decimal before, decimal change, decimal after, int goalsFor, int goalsAgainst, decimal result, long sequence, DateTime processedAt) => new()
    {
        Id = Guid.NewGuid(), MatchId = match.Id, PlayerId = playerId, OpponentPlayerId = opponentId,
        GameId = match.GameId, RatingBefore = before, RatingChange = change, RatingAfter = after,
        GoalsFor = goalsFor, GoalsAgainst = goalsAgainst, ResultScore = result,
        ProcessingSequence = sequence, ProcessedAt = processedAt
    };
}

