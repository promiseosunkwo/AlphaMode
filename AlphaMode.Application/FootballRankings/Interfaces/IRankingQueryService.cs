using AlphaMode.Application.Common.Models;
using AlphaMode.Application.FootballRankings.DTOs;

namespace AlphaMode.Application.FootballRankings.Interfaces;

public interface IRankingQueryService
{
    Task<Result<IReadOnlyList<LeaderboardEntryDto>>> GetLeaderboardAsync(Guid gameId, int take = 100, CancellationToken cancellationToken = default);
    Task<Result<PlayerRatingSummaryDto?>> GetPlayerSummaryAsync(string userId, Guid gameId, CancellationToken cancellationToken = default);
}

