using AlphaMode.Domain.Enums;

namespace AlphaMode.Application.TournamentRegistrations.DTOs;

public class TournamentRegistrationDto
{
    public Guid Id { get; set; }

    public Guid TournamentId { get; set; }

    public string TournamentName { get; set; } = string.Empty;

    public string TournamentSlug { get; set; } = string.Empty;

    public string GameName { get; set; } = string.Empty;

    public Guid PlayerId { get; set; }

    public string GamerTag { get; set; } = string.Empty;

    public string PlayerName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }

    public int RankingPoints { get; set; }

    public TournamentRegistrationStatus Status { get; set; }

    public DateTime RegisteredAt { get; set; }

    public DateTime? CheckedInAt { get; set; }

    public DateTime? DisqualifiedAt { get; set; }

    public string? DisqualificationReason { get; set; }

    public bool PlayerIsActive { get; set; }

    public decimal EntryFee { get; set; }

    public DateTime TournamentStartDate { get; set; }
}
