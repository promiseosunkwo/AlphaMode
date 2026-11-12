using AlphaMode.Application.Common.Models;

namespace AlphaMode.Application.Tests.Tournaments;

public sealed class TournamentDateValidatorTests
{
    private readonly DateTime _start = new(2027, 1, 10, 12, 0, 0, DateTimeKind.Utc);

    [Fact] public void ValidTimeline_Succeeds() => Assert.True(TournamentDateValidator.Validate(_start.AddDays(-10), _start.AddDays(-1), _start, _start.AddDays(2)).Succeeded);
    [Fact] public void EndBeforeStart_Fails() => Assert.False(TournamentDateValidator.Validate(null, null, _start, _start).Succeeded);
    [Fact] public void RegistrationOpenAfterDeadline_Fails() => Assert.False(TournamentDateValidator.Validate(_start.AddDays(-1), _start.AddDays(-2), _start, _start.AddDays(1)).Succeeded);
    [Fact] public void DeadlineAfterTournamentStart_Fails() => Assert.False(TournamentDateValidator.Validate(null, _start.AddMinutes(1), _start, _start.AddDays(1)).Succeeded);
    [Fact] public void RegistrationOpeningAtStart_Fails() => Assert.False(TournamentDateValidator.Validate(_start, null, _start, _start.AddDays(1)).Succeeded);
}
