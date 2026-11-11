namespace AlphaMode.Application.PrizePools.DTOs;

public class PrizePoolDto
{
    public Guid Id { get; set; }

    public Guid TournamentId { get; set; }

    public string TournamentName { get; set; } = string.Empty;

    public decimal TotalPrize { get; set; }

    public string Currency { get; set; } = string.Empty;

    public decimal DistributedAmount { get; set; }

    public decimal RemainingAmount { get; set; }

    public IReadOnlyList<PrizeDistributionDto> Distributions{ get; set; } = [];
}