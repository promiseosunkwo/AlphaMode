using AlphaMode.Domain.Common;

namespace AlphaMode.Domain.Entities;
public class PrizePool : AuditableEntity
{
    public Guid TournamentId { get; set; }

    public Tournament Tournament { get; set; } = null!;

    public decimal TotalPrize { get; set; }

    public string Currency { get; set; } = "NGN";

    public ICollection<PrizeDistribution> Distributions { get; set; }
        = new List<PrizeDistribution>();
}