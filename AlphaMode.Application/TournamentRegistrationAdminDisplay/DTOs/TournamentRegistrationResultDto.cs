using AlphaMode.Domain.Enums;

namespace AlphaMode.Application.TournamentRegistration.DTOs
{
    public class TournamentRegistrationResultDto
    {
        public Guid RegistrationId { get; set; }

        public Guid TournamentId { get; set; }

        public TournamentRegistrationStatus Status { get; set; }

        public Guid? PaymentId { get; set; }

        public bool PaymentRequired { get; set; }

        public decimal AmountDue { get; set; }

        public string Currency { get; set; } = "NGN";
    }
}
