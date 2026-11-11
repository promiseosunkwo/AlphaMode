using AlphaMode.Domain.Enums;

namespace AlphaMode.Application.TournamentRegistrationAdminDisplay.DTOs
{
    public class MyTournamentDto
    {
        public Guid RegistrationId { get; set; }

        public Guid TournamentId { get; set; }

        public string TournamentName { get; set; } = string.Empty;

        public string TournamentSlug { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public Guid GameId { get; set; }

        public string GameName { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public TournamentFormat Format { get; set; }

        public TournamentStatus TournamentStatus { get; set; }

        public TournamentRegistrationStatus RegistrationStatus { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public DateTime RegisteredAt { get; set; }

        public DateTime? CheckedInAt { get; set; }

        public decimal EntryFee { get; set; }

        public int RegisteredPlayers { get; set; }

        public int MaxPlayers { get; set; }

        public decimal PrizePool { get; set; }

        public string Currency { get; set; } = "NGN";

        public Guid? PendingPaymentId { get; set; }

        public bool RequiresPayment =>
            EntryFee > 0;

        public bool IsPaymentPending => RegistrationStatus == TournamentRegistrationStatus.PendingPayment;

        public bool IsRegistered => RegistrationStatus == TournamentRegistrationStatus.Registered;

        public bool IsCheckedIn => RegistrationStatus == TournamentRegistrationStatus.CheckedIn;

        public bool IsFinished => EndDate <= DateTime.UtcNow;
    }
}
