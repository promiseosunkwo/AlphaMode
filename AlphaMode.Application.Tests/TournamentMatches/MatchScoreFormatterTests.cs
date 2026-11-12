using AlphaMode.Application.TournamentMatches.Helpers;

namespace AlphaMode.Application.Tests.TournamentMatches;

public sealed class MatchScoreFormatterTests
{
    [Fact] public void MissingScore_ReturnsDash() => Assert.Equal("—", MatchScoreFormatter.Format(null, 1, false, null, null));
    [Fact] public void RegulationScore_ReturnsCompactScore() => Assert.Equal("2–1", MatchScoreFormatter.Format(2, 1, false, null, null));
    [Fact] public void ExtraTime_AddsAet() => Assert.Equal("2–1 (AET)", MatchScoreFormatter.Format(2, 1, true, null, null));
    [Fact] public void Penalties_IncludeShootoutScore() => Assert.Equal("1–1 (AET), 5–4 pens", MatchScoreFormatter.Format(1, 1, true, 5, 4));
}
