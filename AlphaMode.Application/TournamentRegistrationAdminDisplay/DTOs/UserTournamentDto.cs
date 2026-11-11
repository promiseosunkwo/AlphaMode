using AlphaMode.Domain.Enums;

namespace AlphaMode.Application.TournamentRegistration.DTOs
{
    public class UserTournamentDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public Guid GameId { get; set; }

        public string Game { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public TournamentFormat Format { get; set; }

        public TournamentStatus Status { get; set; }

        public DateTime StartsAt { get; set; }

        public DateTime EndsAt { get; set; }

        public DateTime? RegistrationOpenDate { get; set; }

        public DateTime? RegistrationDeadline { get; set; }

        public decimal EntryFee { get; set; }

        public decimal PrizePool { get; set; }

        public string Currency { get; set; } = "NGN";

        public int RegisteredPlayers { get; set; }

        public int Capacity { get; set; }

        public Guid? RegistrationId { get; set; }

        public TournamentRegistrationStatus? RegistrationStatus { get; set; }

        public Guid? PendingPaymentId { get; set; }

        public bool IsJoined => RegistrationStatus is TournamentRegistrationStatus.Registered or TournamentRegistrationStatus.CheckedIn;

        public bool HasRegistration => RegistrationId.HasValue;

        public bool IsFull => Capacity > 0 && RegisteredPlayers >= Capacity;
    }
}
