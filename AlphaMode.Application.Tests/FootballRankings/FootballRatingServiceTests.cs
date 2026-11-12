using AlphaMode.Application.FootballRankings.Services;
using AlphaMode.Application.Tests.Support;

namespace AlphaMode.Application.Tests.FootballRankings;

public sealed class FootballRatingServiceTests
{
    [Fact]
    public async Task RankedTournamentResult_UpdatesBothRatingsAndWritesTwoLedgerRows()
    {
        await using var db = new TestApplicationDbContext();
        var game = TestData.Game(); var winner = TestData.Player("Winner"); var loser = TestData.Player("Loser"); var tournament = TestData.Tournament(game); var match = TestData.Match(game, winner, loser, tournament);
        db.AddRange(game, winner, loser, tournament, match); await db.SaveChangesAsync();
        var result = await new FootballRatingService(db).ApplyMatchAsync(match); await db.SaveChangesAsync();
        Assert.True(result.Succeeded); Assert.Equal(2, db.FootballRatingHistories.Count());
        Assert.Equal(1016m, db.Rankings.Single(x => x.PlayerId == winner.Id).EloRating); Assert.Equal(984m, db.Rankings.Single(x => x.PlayerId == loser.Id).EloRating);
    }

    [Fact]
    public async Task FriendlyResult_DoesNotCreateRankingOrHistory()
    {
        await using var db = new TestApplicationDbContext();
        var game = TestData.Game(); var one = TestData.Player("One"); var two = TestData.Player("Two"); var match = TestData.Match(game, one, two, ranked: false);
        db.AddRange(game, one, two, match); await db.SaveChangesAsync();
        var result = await new FootballRatingService(db).ApplyMatchAsync(match); await db.SaveChangesAsync();
        Assert.True(result.Succeeded); Assert.Empty(db.Rankings); Assert.Empty(db.FootballRatingHistories);
    }

    [Fact]
    public async Task ProcessingSameMatchTwice_IsIdempotent()
    {
        await using var db = new TestApplicationDbContext();
        var game = TestData.Game(); var one = TestData.Player("One"); var two = TestData.Player("Two"); var tournament = TestData.Tournament(game); var match = TestData.Match(game, one, two, tournament);
        db.AddRange(game, one, two, tournament, match); await db.SaveChangesAsync(); var service = new FootballRatingService(db);
        await service.ApplyMatchAsync(match); await db.SaveChangesAsync(); var second = await service.ApplyMatchAsync(match); await db.SaveChangesAsync();
        Assert.True(second.Succeeded); Assert.Equal(2, db.FootballRatingHistories.Count()); Assert.Equal(1016m, db.Rankings.Single(x => x.PlayerId == one.Id).EloRating);
    }
}
