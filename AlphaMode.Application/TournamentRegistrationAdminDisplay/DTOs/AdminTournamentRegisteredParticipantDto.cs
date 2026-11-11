using AlphaMode.Domain.Enums;

namespace AlphaMode.Application.TournamentRegistrationAdminDisplay.DTOs;

public class AdminTournamentRegisteredParticipantDto
{
    public Guid RegistrationId { get; set; }

    public Guid PlayerId { get; set; }

    public string GamerTag { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }

    public int RankingPoints { get; set; }

    public bool IsPlayerActive { get; set; }

    public TournamentRegistrationStatus Status { get; set; }

    public DateTime RegisteredAt { get; set; }

    public DateTime? CheckedInAt { get; set; }

    public DateTime? DisqualifiedAt { get; set; }

    public string? DisqualificationReason { get; set; }

    public bool IsEligibleForFixtures => Status == TournamentRegistrationStatus.CheckedIn;
}
