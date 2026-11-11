using AlphaMode.Domain.Enums;

namespace AlphaMode.Application.PlayerTournamentRegistrations.DTOs;

public class TournamentPlayerRegistrationDTO
{
    public Guid Id { get; set; }

    public Guid TournamentId { get; set; }

    public string TournamentName { get; set; } = string.Empty;

    public Guid PlayerId { get; set; }

    public string GamerTag { get; set; } = string.Empty;

    public TournamentRegistrationStatus Status { get; set; }

    public DateTime RegisteredAt { get; set; }

    public DateTime? CheckedInAt { get; set; }

    public DateTime? DisqualifiedAt { get; set; }

    public string? DisqualificationReason { get; set; }
}
