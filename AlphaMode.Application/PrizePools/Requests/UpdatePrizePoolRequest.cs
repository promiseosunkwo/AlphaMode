namespace AlphaMode.Application.PrizePools.Requests;

public class UpdatePrizePoolRequest
{
    public Guid Id { get; set; }
    public decimal TotalPrize { get; set; }
    public string Currency { get; set; } = "NGN";
    public IReadOnlyList<PrizeDistributionRequest> Distributions{ get; set; } = [];
}