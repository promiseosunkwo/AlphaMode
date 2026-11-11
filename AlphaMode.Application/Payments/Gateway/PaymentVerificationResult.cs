using AlphaMode.Domain.Enums;

namespace AlphaMode.Application.Payments.Gateway;

public class PaymentVerificationResult
{
    public bool IsSuccessful { get; set; }

    public string TransactionReference { get; set; } = string.Empty;

    public string? ProviderReference { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = string.Empty;

    public PaymentMethod Method { get; set; }
}