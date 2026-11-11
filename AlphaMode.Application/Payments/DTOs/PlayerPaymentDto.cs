using AlphaMode.Domain.Enums;

namespace AlphaMode.Application.Payments.DTOs
{
    public class PlayerPaymentDto
    {
        public Guid Id { get; set; }

        public Guid PlayerId { get; set; }

        public Guid? TournamentId { get; set; }

        public Guid? TournamentRegistrationId { get; set; }

        public string TournamentName { get; set; } = string.Empty;

        public string GameName { get; set; } = string.Empty;

        public string? TournamentImageUrl { get; set; }

        public decimal Amount { get; set; }

        public string Currency { get; set; } = "NGN";

        public PaymentStatus Status { get; set; }

        public PaymentType Type { get; set; }

        public PaymentMethod Method { get; set; }

        public string TransactionReference { get; set; } = string.Empty;

        public string? ProviderReference { get; set; }

        public string? PaymentProvider { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? PaidAt { get; set; }

        public TournamentRegistrationStatus? RegistrationStatus { get; set; }
    }
}
