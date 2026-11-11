namespace AlphaMode.Application.Payments.Gateway;

public class PaymentInitializationRequest
{
    public string Email { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "NGN";

    public string TransactionReference { get; set; } = string.Empty;

    public string CallbackUrl { get; set; } = string.Empty;
}
