using AlphaMode.Application.Common.Interfaces;
using AlphaMode.Application.Common.Models;
using AlphaMode.Application.FootballRankings.DTOs;
using AlphaMode.Application.FootballRankings.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AlphaMode.Application.FootballRankings.Services;

public sealed class RankingQueryService : IRankingQueryService
{
    private readonly IApplicationDbContext _context;
    public RankingQueryService(IApplicationDbContext context) => _context = context;

    public async Task<Result<IReadOnlyList<LeaderboardEntryDto>>> GetLeaderboardAsync(Guid gameId, int take = 100, CancellationToken cancellationToken = default)
    {
        if (gameId == Guid.Empty) return Result<IReadOnlyList<LeaderboardEntryDto>>.Failure("Select a game.");
        take = Math.Clamp(take, 1, 200);
        var rows = await _context.Rankings.AsNoTracking().Where(x => x.GameId == gameId)
            .OrderByDescending(x => x.EloRating).ThenByDescending(x => x.MatchesPlayed).ThenBy(x => x.Player.GamerTag)
            .Take(take)
            .Select(x => new { x.PlayerId, x.Player.UserId, x.Player.GamerTag, x.EloRating, x.MatchesPlayed, x.Wins, x.Draws, x.Losses })
            .ToListAsync(cancellationToken);
        var result = rows.Select((x, index) => new LeaderboardEntryDto { Position = index + 1, PlayerId = x.PlayerId, UserId = x.UserId, GamerTag = x.GamerTag, EloRating = x.EloRating, MatchesPlayed = x.MatchesPlayed, Wins = x.Wins, Draws = x.Draws, Losses = x.Losses }).ToList();
        return Result<IReadOnlyList<LeaderboardEntryDto>>.Success(result);
    }

    public async Task<Result<PlayerRatingSummaryDto?>> GetPlayerSummaryAsync(string userId, Guid gameId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId) || gameId == Guid.Empty) return Result<PlayerRatingSummaryDto?>.Success(null);
        var ranking = await _context.Rankings.AsNoTracking().Where(x => x.GameId == gameId && x.Player.UserId == userId)
            .Select(x => new { x.PlayerId, x.EloRating, x.MatchesPlayed, x.Wins, x.Draws, x.Losses }).FirstOrDefaultAsync(cancellationToken);
        if (ranking is null) return Result<PlayerRatingSummaryDto?>.Success(null);
        var position = await _context.Rankings.AsNoTracking().CountAsync(x => x.GameId == gameId && x.EloRating > ranking.EloRating, cancellationToken) + 1;
        var lastChange = await _context.FootballRatingHistories.AsNoTracking().Where(x => x.GameId == gameId && x.PlayerId == ranking.PlayerId)
            .OrderByDescending(x => x.ProcessingSequence).Select(x => (decimal?)x.RatingChange).FirstOrDefaultAsync(cancellationToken);
        return Result<PlayerRatingSummaryDto?>.Success(new PlayerRatingSummaryDto { Position = position, EloRating = ranking.EloRating, LastChange = lastChange, MatchesPlayed = ranking.MatchesPlayed, Wins = ranking.Wins, Draws = ranking.Draws, Losses = ranking.Losses });
    }
}

