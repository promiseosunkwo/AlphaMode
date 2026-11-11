using AlphaMode.Application.FriendlyMatches.Services;
using AlphaMode.Domain.Enums;
using Xunit;
using FriendlyMatchType = AlphaMode.Domain.Enums.MatchType;

namespace AlphaMode.Application.Tests.FriendlyMatches;

// These tests lock down the boundary that keeps standalone matches valid and non-tournament-only.
public sealed class FriendlyMatchRulesTests
{
    [Theory]
    [InlineData(FriendlyMatchType.Friendly)]
    [InlineData(FriendlyMatchType.Exhibition)]
    [InlineData(FriendlyMatchType.Practice)]
    public void StandaloneTypes_AreAccepted(FriendlyMatchType type)
    {
        var result = FriendlyMatchRules.Validate(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), type);
        Assert.True(result.Succeeded);
    }

    [Fact]
    public void TournamentType_IsRejected()
    {
        var result = FriendlyMatchRules.Validate(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), FriendlyMatchType.Tournament);
        Assert.False(result.Succeeded);
    }

    [Fact]
    public void SamePlayerOnBothSides_IsRejected()
    {
        var playerId = Guid.NewGuid();
        var result = FriendlyMatchRules.Validate(Guid.NewGuid(), playerId, playerId, FriendlyMatchType.Friendly);
        Assert.False(result.Succeeded);
    }
}

