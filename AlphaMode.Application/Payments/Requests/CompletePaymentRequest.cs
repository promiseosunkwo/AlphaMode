namespace AlphaMode.Application.Payments.Requests
{
    public class CompletePaymentRequest
    {
        public Guid PaymentId { get; set; }

        public string? ProviderReference { get; set; }

        public string? PaymentProvider { get; set; }
    }
}
