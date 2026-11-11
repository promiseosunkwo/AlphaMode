namespace AlphaMode.Application.TournamentMatches.Helpers;

public static class MatchScoreFormatter
{
    public static string Format(
        int? playerOneScore,
        int? playerTwoScore,
        bool wentToExtraTime,
        int? playerOnePenaltyScore,
        int? playerTwoPenaltyScore)
    {
        if (!playerOneScore.HasValue || !playerTwoScore.HasValue)
        {
            return "—";
        }

        var score = $"{playerOneScore.Value}–{playerTwoScore.Value}";

        if (playerOnePenaltyScore.HasValue &&
            playerTwoPenaltyScore.HasValue)
        {
            return $"{score} (AET), " +
                   $"{playerOnePenaltyScore.Value}–" +
                   $"{playerTwoPenaltyScore.Value} pens";
        }

        return wentToExtraTime
            ? $"{score} (AET)"
            : score;
    }
}