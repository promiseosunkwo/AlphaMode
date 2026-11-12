using AlphaMode.Application.FootballRankings.Services;
using AlphaMode.Application.Tests.Support;
using AlphaMode.Domain.Entities;

namespace AlphaMode.Application.Tests.FootballRankings;

public sealed class RankingQueryServiceTests
{
    [Fact]
    public async Task Leaderboard_OrdersByEloThenMatchesPlayed()
    {
        await using var db = new TestApplicationDbContext();
        var game = TestData.Game(); var first = TestData.Player("First"); var second = TestData.Player("Second"); var third = TestData.Player("Third");
        db.AddRange(game, first, second, third,
            new Ranking { Id = Guid.NewGuid(), GameId = game.Id, Game = game, PlayerId = first.Id, Player = first, EloRating = 1200, MatchesPlayed = 3 },
            new Ranking { Id = Guid.NewGuid(), GameId = game.Id, Game = game, PlayerId = second.Id, Player = second, EloRating = 1200, MatchesPlayed = 7 },
            new Ranking { Id = Guid.NewGuid(), GameId = game.Id, Game = game, PlayerId = third.Id, Player = third, EloRating = 1100, MatchesPlayed = 20 });
        await db.SaveChangesAsync();
        var result = await new RankingQueryService(db).GetLeaderboardAsync(game.Id);
        Assert.True(result.Succeeded);
        var leaderboard = Assert.IsAssignableFrom<IReadOnlyList<AlphaMode.Application.FootballRankings.DTOs.LeaderboardEntryDto>>(result.Value);
        Assert.Equal(new[] { "Second", "First", "Third" }, leaderboard.Select(x => x.GamerTag));
        Assert.Equal(new[] { 1, 2, 3 }, leaderboard.Select(x => x.Position));
    }

    [Fact]
    public async Task PlayerSummary_IncludesPositionAndLatestChange()
    {
        await using var db = new TestApplicationDbContext();
        var game = TestData.Game(); var player = TestData.Player("Player"); var leader = TestData.Player("Leader");
        db.AddRange(game, player, leader, new Ranking { Id = Guid.NewGuid(), GameId = game.Id, Game = game, PlayerId = player.Id, Player = player, EloRating = 1050, MatchesPlayed = 4 }, new Ranking { Id = Guid.NewGuid(), GameId = game.Id, Game = game, PlayerId = leader.Id, Player = leader, EloRating = 1100, MatchesPlayed = 5 },
            new FootballRatingHistory { Id = Guid.NewGuid(), GameId = game.Id, Game = game, MatchId = Guid.NewGuid(), PlayerId = player.Id, Player = player, OpponentPlayerId = leader.Id, OpponentPlayer = leader, RatingBefore = 1030, RatingChange = 20, RatingAfter = 1050, ProcessingSequence = 2 });
        await db.SaveChangesAsync();
        var result = await new RankingQueryService(db).GetPlayerSummaryAsync(player.UserId, game.Id);
        Assert.True(result.Succeeded); Assert.NotNull(result.Value); Assert.Equal(2, result.Value.Position); Assert.Equal(20, result.Value.LastChange); Assert.True(result.Value.IsProvisional);
    }
}
