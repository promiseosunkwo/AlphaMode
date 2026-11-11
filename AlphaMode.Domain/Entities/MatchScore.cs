using AlphaMode.Domain.Common;

namespace AlphaMode.Domain.Entities;
public class MatchScore : AuditableEntity
{
    public Guid MatchId { get; set; }

    public Match Match { get; set; } = null!;

    public int PlayerOneScore { get; set; }

    public int PlayerTwoScore { get; set; }

    public int? PlayerOnePenaltyScore { get; set; }

    public int? PlayerTwoPenaltyScore { get; set; }

    public bool WentToExtraTime { get; set; }
}