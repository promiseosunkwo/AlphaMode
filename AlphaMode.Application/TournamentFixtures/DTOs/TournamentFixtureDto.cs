using AlphaMode.Domain.Enums;

namespace AlphaMode.Application.TournamentFixtures.DTOs;

public class TournamentFixtureDto
{
    public Guid TournamentId { get; set; }

    public string TournamentName { get; set; } = string.Empty;

    public string GameName { get; set; } = string.Empty;

    public TournamentFormat Format { get; set; }

    public DateTime StartDate { get; set; }

    public int Legs { get; set; }

    public int EligiblePlayers { get; set; }

    public bool FixturesGenerated { get; set; }

    public IReadOnlyList<FixtureRoundDto> Rounds { get; set; }
        = [];
}


public class FixtureRoundDto
{
    public int RoundNumber { get; set; }

    public string Name { get; set; } = string.Empty;

    public IReadOnlyList<FixtureMatchDto> Matches { get; set; }
        = [];
}



public class FixtureMatchDto
{
    public Guid? MatchId { get; set; }

    public Guid? PlayerOneId { get; set; }

    public string PlayerOneName { get; set; } = string.Empty;

    public Guid? PlayerTwoId { get; set; }

    public string PlayerTwoName { get; set; } = string.Empty;

    public int RoundNumber { get; set; }

    public int? MatchWeek { get; set; }

    public MatchStatus Status { get; set; }

    public DateTime? ScheduledAt { get; set; }

    public Guid? WinnerId { get; set; }

    public int? PlayerOneScore { get; set; }

    public int? PlayerTwoScore { get; set; }

    public bool WentToExtraTime { get; set; }
    public int? PlayerOnePenaltyScore { get; set; }
    public int? PlayerTwoPenaltyScore { get; set; }

    public bool HasScore => PlayerOneScore.HasValue && PlayerTwoScore.HasValue;

    public bool IsBye => !PlayerOneId.HasValue || !PlayerTwoId.HasValue;
}
