using AlphaMode.Application.Common.Models;
using AlphaMode.Domain.Enums;
using FriendlyMatchType = AlphaMode.Domain.Enums.MatchType;

namespace AlphaMode.Application.FriendlyMatches.Services;

// Central rules keep UI and service validation aligned without allowing friendlies into ranked workflows.
public static class FriendlyMatchRules
{
    public static Result Validate(Guid gameId, Guid playerOneId, Guid playerTwoId, FriendlyMatchType type)
    {
        if (gameId == Guid.Empty || playerOneId == Guid.Empty || playerTwoId == Guid.Empty)
            return Result.Failure("Game and both players are required.");
        if (playerOneId == playerTwoId)
            return Result.Failure("A player cannot play a friendly against themselves.");
        if (type is not (FriendlyMatchType.Friendly or FriendlyMatchType.Exhibition or FriendlyMatchType.Practice))
            return Result.Failure("Select a friendly, exhibition, or practice match type.");
        return Result.Success();
    }
}

