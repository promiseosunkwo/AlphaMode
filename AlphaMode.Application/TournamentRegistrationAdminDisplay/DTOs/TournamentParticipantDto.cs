using AlphaMode.Domain.Enums;

namespace AlphaMode.Application.TournamentRegistrationAdminDisplay.DTOs
{
    public class TournamentParticipantDto
    {
        public Guid PlayerId { get; set; }

        public string GamerTag { get; set; } = string.Empty;

        public string? AvatarUrl { get; set; }

        public int RankingPoints { get; set; }

        public TournamentRegistrationStatus Status { get; set; }

        public DateTime? CheckedInAt { get; set; }
    }
}
