using AlphaMode.Application.Common.Models;
using AlphaMode.Domain.Entities;
using AlphaMode.Domain.Enums;

namespace AlphaMode.Application.FootballRankings.Services;

public static class RankedMatchEligibilityPolicy
{
    public static Result Validate(Match match)
    {
        // Elo is deliberately opt-in. Friendlies and other standalone matches remain recorded but unranked.
        if (!match.IsRanked)
            return Result.Failure("This match is not ranked.");
        if (match.Type != AlphaMode.Domain.Enums.MatchType.Tournament || !match.TournamentId.HasValue)
            return Result.Failure("Only tournament matches can affect Elo.");
        if (match.Status != MatchStatus.Completed)
            return Result.Failure("Only completed matches can affect Elo.");
        // A walkover or double forfeiture awards tournament progression, not a skill result.
        if (match.ResultType != MatchResultType.Played)
            return Result.Failure("Forfeited matches do not affect Elo.");
        if (!match.PlayerOneId.HasValue || !match.PlayerTwoId.HasValue || match.PlayerOneId == match.PlayerTwoId)
            return Result.Failure("Two different players are required for Elo.");
        if (match.GameId == Guid.Empty || match.Score is null)
            return Result.Failure("A game and score are required for Elo.");
        return Result.Success();
    }
}

