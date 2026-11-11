using AlphaMode.Application.Common.Models;
using AlphaMode.Domain.Enums;

namespace AlphaMode.Application.TournamentMatches.Services;

public static class MatchScoreValidator
{
    public static Result Validate(
        TournamentFormat format,
        int playerOneScore,
        int playerTwoScore,
        bool wentToExtraTime,
        int? playerOnePenaltyScore,
        int? playerTwoPenaltyScore)
    {
        if (playerOneScore < 0 || playerTwoScore < 0)
        {
            return Result.Failure("Match scores cannot be negative.");
        }

        bool hasPlayerOnePenalties = playerOnePenaltyScore.HasValue;
        bool hasPlayerTwoPenalties = playerTwoPenaltyScore.HasValue;

        if (hasPlayerOnePenalties != hasPlayerTwoPenalties)
        {
            return Result.Failure(
                "Penalty scores must be provided for both players.");
        }

        bool hasPenalties = hasPlayerOnePenalties && hasPlayerTwoPenalties;

        if (format == TournamentFormat.League)
        {
            if (wentToExtraTime || hasPenalties)
            {
                return Result.Failure(
                    "League matches cannot have extra time or penalty shootouts.");
            }

            return Result.Success();
        }

        if (format != TournamentFormat.SingleElimination &&
            format != TournamentFormat.DoubleElimination)
        {
            return Result.Failure("Unsupported tournament format.");
        }

        if (hasPenalties)
        {
            if (!wentToExtraTime)
            {
                return Result.Failure(
                    "A penalty shootout must follow extra time.");
            }

            if (playerOneScore != playerTwoScore)
            {
                return Result.Failure(
                    "Penalties are only allowed when the match score is tied.");
            }

            if (playerOnePenaltyScore < 0 || playerTwoPenaltyScore < 0)
            {
                return Result.Failure("Penalty scores cannot be negative.");
            }

            if (playerOnePenaltyScore == playerTwoPenaltyScore)
            {
                return Result.Failure(
                    "A penalty shootout must have a winner.");
            }

            return Result.Success();
        }

        if (playerOneScore == playerTwoScore)
        {
            return Result.Failure(
                "A knockout match that ends level requires penalty scores.");
        }

        return Result.Success();
    }
}