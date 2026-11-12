using AlphaMode.Domain.Entities;
using AlphaMode.Domain.Enums;
using AlphaMode.Domain.Identity;
using DomainMatchType = AlphaMode.Domain.Enums.MatchType;

namespace AlphaMode.Application.Tests.Support;

internal static class TestData
{
    public static Player Player(string tag, bool active = true)
    {
        var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), UserName = $"{tag}@test.local", GamerTag = tag, FirstName = tag, LastName = "Player" };
        return new Player { Id = Guid.NewGuid(), UserId = user.Id, User = user, GamerTag = tag, IsActive = active };
    }

    public static Game Game(string name = "EA FC") => new() { Id = Guid.NewGuid(), Name = name, Slug = name.ToLowerInvariant().Replace(' ', '-'), Version = "1", Platform = "PC", IsActive = true };

    public static Tournament Tournament(Game game, TournamentStatus status = TournamentStatus.Live) => new()
    {
        Id = Guid.NewGuid(), Name = $"{game.Name} Cup", Slug = $"{game.Slug}-cup", Description = "Test tournament", GameId = game.Id, Game = game,
        Format = TournamentFormat.SingleElimination, Status = status, StartDate = DateTime.UtcNow.AddDays(1), EndDate = DateTime.UtcNow.AddDays(2), MaxPlayers = 16
    };

    public static Match Match(Game game, Player one, Player two, Tournament? tournament = null, MatchStatus status = MatchStatus.Completed, bool ranked = true) => new()
    {
        Id = Guid.NewGuid(), GameId = game.Id, Game = game, TournamentId = tournament?.Id, Tournament = tournament,
        Type = tournament is null ? DomainMatchType.Friendly : DomainMatchType.Tournament, IsRanked = ranked, PlayerOneId = one.Id, PlayerOne = one,
        PlayerTwoId = two.Id, PlayerTwo = two, Status = status, ResultType = MatchResultType.Played, ScheduledAt = DateTime.UtcNow,
        WinnerId = one.Id, Score = new MatchScore { Id = Guid.NewGuid(), PlayerOneScore = 2, PlayerTwoScore = 1 }
    };
}
