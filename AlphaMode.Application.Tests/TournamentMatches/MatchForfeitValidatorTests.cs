using AlphaMode.Application.Tests.Support;
using AlphaMode.Application.TournamentMatches.Requests;
using AlphaMode.Application.TournamentMatches.Services;
using AlphaMode.Domain.Enums;

namespace AlphaMode.Application.Tests.TournamentMatches;

public sealed class MatchForfeitValidatorTests
{
    [Fact]
    public void ValidWalkover_Succeeds()
    {
        var game = TestData.Game(); var one = TestData.Player("One"); var two = TestData.Player("Two"); var tournament = TestData.Tournament(game); var match = TestData.Match(game, one, two, tournament, MatchStatus.Scheduled); match.Score = null;
        var result = MatchForfeitValidator.Validate(match, new ForfeitMatchRequest { ResultType = MatchResultType.Walkover, ForfeitingPlayerId = two.Id, Reason = MatchForfeitReason.NoShow, Notes = "Player did not attend." });
        Assert.True(result.Succeeded);
    }

    [Fact] public void CompletedMatch_Fails() { var (m, r) = ValidObjects(); m.Status = MatchStatus.Completed; Assert.False(MatchForfeitValidator.Validate(m, r).Succeeded); }
    [Fact] public void PlayerOutsideMatch_Fails() { var (m, r) = ValidObjects(); r.ForfeitingPlayerId = Guid.NewGuid(); Assert.False(MatchForfeitValidator.Validate(m, r).Succeeded); }
    [Fact] public void DoubleForfeitOutsideLeague_Fails() { var (m, r) = ValidObjects(); r.ResultType = MatchResultType.DoubleForfeit; r.ForfeitingPlayerId = null; Assert.False(MatchForfeitValidator.Validate(m, r).Succeeded); }
    [Fact] public void MissingNotes_Fails() { var (m, r) = ValidObjects(); r.Notes = " "; Assert.False(MatchForfeitValidator.Validate(m, r).Succeeded); }

    private static (AlphaMode.Domain.Entities.Match, ForfeitMatchRequest) ValidObjects()
    {
        var game = TestData.Game(); var one = TestData.Player("One"); var two = TestData.Player("Two"); var tournament = TestData.Tournament(game); var match = TestData.Match(game, one, two, tournament, MatchStatus.Scheduled); match.Score = null;
        return (match, new ForfeitMatchRequest { ResultType = MatchResultType.Walkover, ForfeitingPlayerId = two.Id, Reason = MatchForfeitReason.NoShow, Notes = "No show." });
    }
}
