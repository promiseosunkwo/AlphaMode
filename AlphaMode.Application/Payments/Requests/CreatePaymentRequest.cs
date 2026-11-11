using AlphaMode.Domain.Enums;

namespace AlphaMode.Application.Payments.Requests
{
    public class CreatePaymentRequest
    {
        public Guid PlayerId { get; set; }

        public Guid? TournamentId { get; set; }

        public Guid? TournamentRegistrationId { get; set; }

        public decimal Amount { get; set; }

        public string Currency { get; set; } = "NGN";

        public PaymentType Type { get; set; }

        public PaymentMethod Method { get; set; }

        public string? PaymentProvider { get; set; }
    }
}
