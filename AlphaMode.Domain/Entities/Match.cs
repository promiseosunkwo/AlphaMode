using AlphaMode.Domain.Common;
using AlphaMode.Domain.Enums;

namespace AlphaMode.Domain.Entities;
public class Match : AuditableEntity
{
    // A match used to require a tournament, which made standalone friendlies impossible.
    // TournamentId is now optional while tournament services still enforce it for competitive fixtures.
    public Guid? TournamentId { get; set; }

    public Tournament? Tournament { get; set; }

    // Standalone matches cannot discover their game through a tournament, so every match now owns
    // an explicit game relationship. This also makes match history filtering consistent.
    public Guid GameId { get; set; }

    public Game Game { get; set; } = null!;

    public Enums.MatchType Type { get; set; }

    public bool IsRanked { get; set; }

    public Guid? PlayerOneId { get; set; }

    public Player? PlayerOne { get; set; }

    public Guid? PlayerTwoId { get; set; }

    public Player? PlayerTwo { get; set; }

    public int RoundNumber { get; set; }

    public MatchStatus Status { get; set; }

    public Guid? WinnerId { get; set; }

    public Player? Winner { get; set; }

    public MatchScore? Score { get; set; }

    public DateTime? ScheduledAt { get; set; }

    public int? MatchWeek { get; set; }

    public MatchResultType ResultType { get; set; } = MatchResultType.Played;

    public MatchForfeitReason? ForfeitReason { get; set; }

    public Guid? ForfeitingPlayerId { get; set; }

    public string? ForfeitNotes { get; set; }

    public DateTime? ForfeitedAt { get; set; }
}

