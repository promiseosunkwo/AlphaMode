using AlphaMode.Domain.Enums;

namespace AlphaMode.Application.Tournaments.DTOs;

public class UpdateTournamentRequest
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid GameId { get; set; }

    public TournamentFormat Format { get; set; }

    public DateTime TournamentStartDate { get; set; }

    public DateTime TournamentEndDate { get; set; }

    public DateTime? RegistrationOpenDate { get; set; }
    public DateTime? RegistrationDeadline { get; set; }


    public int MaxPlayers { get; set; }

    public decimal EntryFee { get; set; }

    public int Legs { get; set; }

    public int PointsForWin { get; set; }

    public int PointsForDraw { get; set; }

    public int PointsForLoss { get; set; }

    public string? ImageUrl { get; set; }

    public TournamentStatus Status { get; set; }
}