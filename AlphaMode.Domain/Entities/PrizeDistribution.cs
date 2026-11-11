using AlphaMode.Domain.Common;

namespace AlphaMode.Domain.Entities;

public class PrizeDistribution : AuditableEntity
{
    public Guid PrizePoolId { get; set; }

    public PrizePool PrizePool { get; set; } = null!;

    public int Position { get; set; }

    public decimal Amount { get; set; }
}
