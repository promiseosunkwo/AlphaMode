using AlphaMode.Application.TournamentMatches.Services;
using AlphaMode.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AlphaMode.Application.Tests.TournamentMatches;

public class MatchScoreValidatorTests
{
    [Fact]
    public void League_DrawWithoutPenalties_IsValid()
    {
        var result = MatchScoreValidator.Validate(
            TournamentFormat.League, 2, 2, false, null, null);

        Assert.True(result.Succeeded);
    }

    [Fact]
    public void League_WithPenalties_IsRejected()
    {
        var result = MatchScoreValidator.Validate(
            TournamentFormat.League, 2, 2, true, 5, 4);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public void Knockout_WithNormalWinner_IsValid()
    {
        var result = MatchScoreValidator.Validate(
            TournamentFormat.SingleElimination, 3, 1, false, null, null);

        Assert.True(result.Succeeded);
    }

    [Fact]
    public void Knockout_DrawWithPenaltyWinner_IsValid()
    {
        var result = MatchScoreValidator.Validate(
            TournamentFormat.SingleElimination, 2, 2, true, 5, 4);

        Assert.True(result.Succeeded);
    }

    [Fact]
    public void Knockout_DrawWithoutPenalties_IsRejected()
    {
        var result = MatchScoreValidator.Validate(
            TournamentFormat.SingleElimination, 2, 2, true, null, null);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public void Knockout_EqualPenaltyScores_IsRejected()
    {
        var result = MatchScoreValidator.Validate(
            TournamentFormat.SingleElimination, 2, 2, true, 4, 4);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public void Knockout_PenaltiesWithoutExtraTime_IsRejected()
    {
        var result = MatchScoreValidator.Validate(
            TournamentFormat.SingleElimination, 2, 2, false, 5, 4);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public void Knockout_PenaltiesWhenMainScoreHasWinner_IsRejected()
    {
        var result = MatchScoreValidator.Validate(
            TournamentFormat.SingleElimination, 3, 2, true, 5, 4);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public void Knockout_OnlyOnePenaltyScore_IsRejected()
    {
        var result = MatchScoreValidator.Validate(
            TournamentFormat.SingleElimination, 2, 2, true, 5, null);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public void Knockout_NegativePenaltyScore_IsRejected()
    {
        var result = MatchScoreValidator.Validate(
            TournamentFormat.SingleElimination, 2, 2, true, -1, 4);

        Assert.False(result.Succeeded);
    }
}
