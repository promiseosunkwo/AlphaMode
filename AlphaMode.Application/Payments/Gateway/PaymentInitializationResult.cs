

namespace AlphaMode.Application.Payments.Gateway;
public class PaymentInitializationResult
{
    public string AuthorizationUrl { get; set; } = string.Empty;

    public string? ProviderReference { get; set; }

    public string Provider { get; set; } = string.Empty;
}