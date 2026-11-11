using AlphaMode.Domain.Common;

namespace AlphaMode.Domain.Entities;

public class FootballRatingHistory : AuditableEntity
{
    public Guid MatchId { get; set; }

    public Match Match { get; set; } = null!;

    public Guid PlayerId { get; set; }

    public Player Player { get; set; } = null!;

    public Guid OpponentPlayerId { get; set; }

    public Player OpponentPlayer { get; set; } = null!;

    public Guid GameId { get; set; }

    public Game Game { get; set; } = null!;

    public decimal RatingBefore { get; set; }

    public decimal RatingChange { get; set; }

    public decimal RatingAfter { get; set; }

    public int GoalsFor { get; set; }

    public int GoalsAgainst { get; set; }

    // 1 = Win, 0.5 = Draw, 0 = Loss
    public decimal ResultScore { get; set; }

    // Deterministic order for replaying rating changes.
    public long ProcessingSequence { get; set; }

    public DateTime ProcessedAt { get; set; }
}