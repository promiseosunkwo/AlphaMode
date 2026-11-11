using AlphaMode.Application.TournamentRegistrationAdminDisplay.DTOs;
using AlphaMode.Domain.Enums;

namespace AlphaMode.Components.Pages.UserPages;

public class MyTournamentDetailsDto
{
    public Guid RegistrationId { get; set; }

    public Guid TournamentId { get; set; }

    public string TournamentName { get; set; } = string.Empty;

    public string TournamentSlug { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string GameName { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }

    public TournamentFormat Format { get; set; }

    public TournamentStatus TournamentStatus { get; set; }

    public TournamentRegistrationStatus RegistrationStatus { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public DateTime? RegistrationDeadline { get; set; }

    public DateTime? CheckedInAt { get; set; }

    public decimal EntryFee { get; set; }

    public int MaxPlayers { get; set; }

    public int RegisteredPlayers { get; set; }

    public int CheckedInPlayers { get; set; }

    public int Legs { get; set; }

    public int PointsForWin { get; set; }

    public int PointsForDraw { get; set; }

    public int PointsForLoss { get; set; }

    public decimal PrizePool { get; set; }

    public string Currency { get; set; } = "NGN";

    public IReadOnlyList<TournamentParticipantDto> Participants { get; set; } = [];
}
