namespace AlphaMode.Application.PrizePools.Requests;

public class PrizeDistributionRequest
{
    public Guid? Id { get; set; }
    public int Position { get; set; }
    public decimal Amount { get; set; }
}