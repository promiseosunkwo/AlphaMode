using AlphaMode.Domain.Enums;

namespace AlphaMode.Application.Tournaments.DTOs;

public class TournamentDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public Guid GameId { get; set; }

    public string GameName { get; set; } = string.Empty;

    public TournamentFormat Format { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public DateTime? RegistrationOpenDate { get; set; }

    public DateTime? RegistrationDeadline { get; set; }

    public decimal EntryFee { get; set; }

    public int MaxPlayers { get; set; }

    public int RegisteredPlayers { get; set; }

    public int Legs { get; set; }

    public int PointsForWin { get; set; }

    public int PointsForDraw { get; set; }

    public int PointsForLoss { get; set; }

    public string Slug { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }

    public TournamentStatus Status { get; set; }
}