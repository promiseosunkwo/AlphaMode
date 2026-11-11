namespace AlphaMode.Application.Payments.DTOs;
public class PaymentCheckoutDto
{
    public Guid PaymentId { get; set; }

    public Guid TournamentRegistrationId { get; set; }

    public string TransactionReference { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "NGN";

    public string TournamentName { get; set; } = string.Empty;

    public bool AlreadyExists { get; set; }
}
