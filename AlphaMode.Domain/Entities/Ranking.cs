using AlphaMode.Domain.Common;


namespace AlphaMode.Domain.Entities;
public class Ranking : AuditableEntity
{
    public Guid PlayerId { get; set; }

    public Player Player { get; set; } = null!;

    public Guid GameId { get; set; }

    public Game Game { get; set; } = null!;

    public int Points { get; set; }

    // Competitive rating
    public decimal EloRating { get; set; } = 1000m;

    // Match statistics
    public int MatchesPlayed { get; set; }

    public int Wins { get; set; }

    public int Draws { get; set; }

    public int Losses { get; set; }

    public int GoalsFor { get; set; }

    public int GoalsAgainst { get; set; }

    public DateTime? LastRankedMatchAt { get; set; }

    public int GoalDifference => GoalsFor - GoalsAgainst;

    public decimal WinRate => MatchesPlayed == 0 ? 0m : Math.Round((decimal)Wins / MatchesPlayed * 100m, 2);
}