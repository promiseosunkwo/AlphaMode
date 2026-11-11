using AlphaMode.Application.Common.Models;
using AlphaMode.Domain.Entities;

namespace AlphaMode.Application.FootballRankings.Interfaces;

public interface IFootballRatingService
{
    Task<Result> ApplyMatchAsync(Match match, CancellationToken cancellationToken = default);
    Task<Result> RebuildGameAsync(Guid gameId, CancellationToken cancellationToken = default);
}

