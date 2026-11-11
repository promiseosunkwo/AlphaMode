namespace AlphaMode.Application.FootballRankings.Services;

public static class EloCalculator
{
    public const decimal InitialRating = 1000m;

    public const int KFactor = 32;

    public static EloCalculationResult Calculate(
        decimal playerOneRating,
        decimal playerTwoRating,
        int playerOneScore,
        int playerTwoScore)
    {
        if (playerOneRating < 0 || playerTwoRating < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(playerOneRating), "Ratings cannot be negative.");
        }

        if (playerOneScore < 0 || playerTwoScore < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(playerOneScore), "Match scores cannot be negative.");
        }

        decimal playerOneResult = playerOneScore > playerTwoScore ? 1m : playerOneScore < playerTwoScore ? 0m : 0.5m;

        decimal playerTwoResult = 1m - playerOneResult;

        double expectedOne = 1.0 /
            (1.0 + Math.Pow(
                10.0,
                (double)(playerTwoRating - playerOneRating) / 400.0));

        decimal expectedOneDecimal = (decimal)expectedOne;
        decimal expectedTwoDecimal = 1m - expectedOneDecimal;

        decimal changeOne = Math.Round(KFactor * (playerOneResult - expectedOneDecimal), 4, MidpointRounding.AwayFromZero);

        decimal changeTwo = -changeOne;

        return new EloCalculationResult(
            playerOneRating,
            playerTwoRating,
            playerOneRating + changeOne,
            playerTwoRating + changeTwo,
            changeOne,
            changeTwo,
            playerOneResult,
            playerTwoResult);
    }
}

public sealed record EloCalculationResult(
    decimal PlayerOneRatingBefore,
    decimal PlayerTwoRatingBefore,
    decimal PlayerOneRatingAfter,
    decimal PlayerTwoRatingAfter,
    decimal PlayerOneChange,
    decimal PlayerTwoChange,
    decimal PlayerOneResult,
    decimal PlayerTwoResult);
