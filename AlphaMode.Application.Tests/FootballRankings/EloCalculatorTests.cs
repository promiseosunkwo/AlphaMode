using AlphaMode.Application.FootballRankings.Services;
using Xunit;

public class EloCalculatorTests
{
    [Fact]
    public void EqualRatedPlayers_WinnerGains16Points()
    {
        var result = EloCalculator.Calculate(1000m, 1000m, 3, 1);

        Assert.Equal(1016m, result.PlayerOneRatingAfter);
        Assert.Equal(984m, result.PlayerTwoRatingAfter);
    }

    [Fact]
    public void EqualRatedPlayers_DrawDoesNotChangeRatings()
    {
        var result = EloCalculator.Calculate(1000m, 1000m, 2, 2);

        Assert.Equal(1000m, result.PlayerOneRatingAfter);
        Assert.Equal(1000m, result.PlayerTwoRatingAfter);
    }

    [Fact]
    public void LowerRatedPlayer_WinsMoreRatingForUpset()
    {
        var upset = EloCalculator.Calculate(1000m, 1200m, 2, 1);

        var expectedWin = EloCalculator.Calculate(1200m, 1000m, 2, 1);

        Assert.True(upset.PlayerOneChange > expectedWin.PlayerOneChange);
    }

    [Fact]
    public void GoalMargin_DoesNotAffectEloChange()
    {
        var narrowWin = EloCalculator.Calculate(1000m, 1000m, 2, 1);

        var largeWin = EloCalculator.Calculate(1000m, 1000m, 7, 0);

        Assert.Equal(narrowWin.PlayerOneChange, largeWin.PlayerOneChange);
    }

    [Fact]
    public void RatingChanges_AreZeroSum()
    {
        var result = EloCalculator.Calculate(1100m, 1300m, 1, 0);

        Assert.Equal(0m, result.PlayerOneChange + result.PlayerTwoChange);
    }

    [Fact]
    public void ExplicitOutcome_AllowsPenaltyWinnerToReceiveWin()
    {
        // Regulation goals may be tied, so the result overload lets the match engine pass
        // the shootout winner without introducing goal-margin weighting.
        var result = EloCalculator.Calculate(1000m, 1000m, 1m);

        Assert.Equal(1016m, result.PlayerOneRatingAfter);
        Assert.Equal(984m, result.PlayerTwoRatingAfter);
    }

    [Theory]
    [InlineData(-1, 2)]
    [InlineData(2, -1)]
    public void NegativeScores_AreRejected(
        int playerOneScore,
        int playerTwoScore)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => EloCalculator.Calculate(
                1000m,
                1000m,
                playerOneScore,
                playerTwoScore));
    }
}

