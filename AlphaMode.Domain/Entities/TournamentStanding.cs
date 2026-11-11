using AlphaMode.Domain.Common;


namespace AlphaMode.Domain.Entities;

public class TournamentStanding : AuditableEntity
{
    public Guid TournamentId { get; set; }

    public Tournament Tournament { get; set; } = null!;

    public Guid PlayerId { get; set; }

    public Player Player { get; set; } = null!;

    public int Played { get; set; }

    public int Wins { get; set; }

    public int Draws { get; set; }

    public int Losses { get; set; }

    public int ScoreFor { get; set; }

    public int ScoreAgainst { get; set; }

    public int Points { get; set; }

    public int ScoreDifference => ScoreFor - ScoreAgainst;
}
