using AlphaMode.Application.FootballRankings.Services;
using AlphaMode.Domain.Entities;
using AlphaMode.Domain.Enums;
using Xunit;
using CompetitiveMatchType = AlphaMode.Domain.Enums.MatchType;

namespace AlphaMode.Application.Tests.FootballRankings;

// Eligibility tests protect the product rule that only genuinely played competitive matches affect Elo.
public sealed class RankedMatchEligibilityPolicyTests
{
    [Fact]
    public void CompletedRankedTournamentMatch_IsEligible()
    {
        var result = RankedMatchEligibilityPolicy.Validate(CreateEligibleMatch());
        Assert.True(result.Succeeded);
    }

    [Fact]
    public void Friendly_IsNeverEligible()
    {
        var match = CreateEligibleMatch();
        match.Type = CompetitiveMatchType.Friendly;
        match.TournamentId = null;
        var result = RankedMatchEligibilityPolicy.Validate(match);
        Assert.False(result.Succeeded);
    }

    [Theory]
    [InlineData(MatchResultType.Walkover)]
    [InlineData(MatchResultType.DoubleForfeit)]
    public void Forfeiture_IsNeverEligible(MatchResultType resultType)
    {
        var match = CreateEligibleMatch();
        match.ResultType = resultType;
        var result = RankedMatchEligibilityPolicy.Validate(match);
        Assert.False(result.Succeeded);
    }

    [Fact]
    public void ExplicitlyUnrankedMatch_IsNotEligible()
    {
        var match = CreateEligibleMatch();
        match.IsRanked = false;
        var result = RankedMatchEligibilityPolicy.Validate(match);
        Assert.False(result.Succeeded);
    }

    private static Match CreateEligibleMatch() => new()
    {
        Id = Guid.NewGuid(),
        TournamentId = Guid.NewGuid(),
        GameId = Guid.NewGuid(),
        PlayerOneId = Guid.NewGuid(),
        PlayerTwoId = Guid.NewGuid(),
        Type = CompetitiveMatchType.Tournament,
        IsRanked = true,
        Status = MatchStatus.Completed,
        ResultType = MatchResultType.Played,
        Score = new MatchScore { PlayerOneScore = 2, PlayerTwoScore = 1 }
    };
}

