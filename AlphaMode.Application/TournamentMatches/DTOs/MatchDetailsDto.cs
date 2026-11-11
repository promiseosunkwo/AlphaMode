using AlphaMode.Domain.Enums;

namespace AlphaMode.Application.TournamentMatches.DTOs;

public class MatchDetailsDto
{
    public Guid Id { get; set; }

    public Guid TournamentId { get; set; }

    public string TournamentName { get; set; } = string.Empty;

    public TournamentFormat TournamentFormat { get; set; }

    public Guid? PlayerOneId { get; set; }

    public string PlayerOneName { get; set; } = "TBD";

    public Guid? PlayerTwoId { get; set; }

    public string PlayerTwoName { get; set; } = "TBD";

    public int RoundNumber { get; set; }

    public int? MatchWeek { get; set; }

    public MatchStatus Status { get; set; }

    public DateTime? ScheduledAt { get; set; }

    public Guid? WinnerId { get; set; }

    public MatchResultType ResultType { get; set; } = MatchResultType.Played;

    public MatchForfeitReason? ForfeitReason { get; set; }

    public Guid? ForfeitingPlayerId { get; set; }

    public string? ForfeitNotes { get; set; }

    public DateTime? ForfeitedAt { get; set; }

    public int? PlayerOneScore { get; set; }

    public int? PlayerTwoScore { get; set; }

    public bool WentToExtraTime { get; set; }

    public int? PlayerOnePenaltyScore { get; set; }

    public int? PlayerTwoPenaltyScore { get; set; }

    public bool HasScore =>
        PlayerOneScore.HasValue &&
        PlayerTwoScore.HasValue;

    public bool IsBye =>
        !PlayerOneId.HasValue ||
        !PlayerTwoId.HasValue;

    public bool IsWalkover =>
    ResultType == MatchResultType.Walkover;

    public bool IsDoubleForfeit =>
        ResultType == MatchResultType.DoubleForfeit;

    public bool IsForfeited =>
        IsWalkover || IsDoubleForfeit;
}
