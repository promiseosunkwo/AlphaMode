using AlphaMode.Domain.Enums;

namespace AlphaMode.Application.Tournaments.DTOs;

public class CreateTournamentRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; } = string.Empty;

    public Guid GameId { get; set; }

    public TournamentFormat Format { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public DateTime? RegistrationOpenDate { get; set; }

    public DateTime? RegistrationDeadline { get; set; }

    public decimal EntryFee { get; set; }

    public int MaxPlayers { get; set; }

    public TournamentStatus Status { get; set; }

    public string? ImageUrl { get; set; }

    public int Legs { get; set; } = 1;

    public int PointsForWin { get; set; } = 3;

    public int PointsForDraw { get; set; } = 1;

    public int PointsForLoss { get; set; }
}