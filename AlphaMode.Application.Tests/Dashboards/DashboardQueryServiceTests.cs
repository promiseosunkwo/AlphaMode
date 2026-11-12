using AlphaMode.Application.Dashboards.Services;
using AlphaMode.Application.Tests.Support;
using AlphaMode.Domain.Enums;

namespace AlphaMode.Application.Tests.Dashboards;

public sealed class DashboardQueryServiceTests
{
    [Fact]
    public async Task PublicMatches_ExcludeFriendliesDraftsAndCancelledTournaments()
    {
        await using var db = new TestApplicationDbContext();
        var game = TestData.Game(); var one = TestData.Player("One"); var two = TestData.Player("Two");
        var live = TestData.Tournament(game, TournamentStatus.Live); var draft = TestData.Tournament(game, TournamentStatus.Draft); var cancelled = TestData.Tournament(game, TournamentStatus.Cancelled);
        var visible = TestData.Match(game, one, two, live); var friendly = TestData.Match(game, one, two); var draftMatch = TestData.Match(game, one, two, draft); var cancelledMatch = TestData.Match(game, one, two, cancelled);
        db.AddRange(game, one, two, live, draft, cancelled, visible, friendly, draftMatch, cancelledMatch); await db.SaveChangesAsync();
        var result = await new DashboardQueryService(db).GetPublicTournamentMatchesAsync();
        Assert.True(result.Succeeded); Assert.Equal(visible.Id, Assert.Single(result.Value!).Id);
    }

    [Fact]
    public async Task UserMatches_ReturnTournamentAndFriendlyMatchesForOnlyThatPlayer()
    {
        await using var db = new TestApplicationDbContext();
        var game = TestData.Game(); var player = TestData.Player("Player"); var opponent = TestData.Player("Opponent"); var outsider = TestData.Player("Outsider"); var tournament = TestData.Tournament(game);
        var tournamentMatch = TestData.Match(game, player, opponent, tournament); var friendly = TestData.Match(game, opponent, player); var unrelated = TestData.Match(game, outsider, opponent, tournament);
        db.AddRange(game, player, opponent, outsider, tournament, tournamentMatch, friendly, unrelated); await db.SaveChangesAsync();
        var result = await new DashboardQueryService(db).GetUserMatchesAsync(player.UserId);
        Assert.True(result.Succeeded); Assert.Equal(2, result.Value!.Count); Assert.Contains(result.Value, x => x.IsFriendly); Assert.Contains(result.Value, x => !x.IsFriendly);
    }

    [Fact]
    public async Task UnknownUser_ReturnsFailure()
    {
        await using var db = new TestApplicationDbContext();
        var result = await new DashboardQueryService(db).GetUserMatchesAsync("missing");
        Assert.False(result.Succeeded);
    }
}
