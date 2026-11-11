namespace AlphaMode.Application.PrizePools.Requests;

public class CreatePrizePoolRequest
{
    public Guid TournamentId { get; set; }
    public decimal TotalPrize { get; set; }
    public string Currency { get; set; } = "NGN";
    public IReadOnlyList<PrizeDistributionRequest> Distributions{ get; set; } = [];
}