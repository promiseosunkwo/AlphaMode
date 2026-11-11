using AlphaMode.Domain.Common;
using AlphaMode.Domain.Enums;

namespace AlphaMode.Domain.Entities;
public class Tournament : AuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public Guid GameId { get; set; }

    public Game Game { get; set; } = null!;

    public TournamentFormat Format { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public DateTime? RegistrationOpenDate { get; set; }

    public DateTime? RegistrationDeadline { get; set; }

    public decimal EntryFee { get; set; }

    public int MaxPlayers { get; set; }

    public int Legs { get; set; } = 1;

    public int PointsForWin { get; set; } = 3;

    public int PointsForDraw { get; set; } = 1;

    public int PointsForLoss { get; set; } = 0;

    public string Slug { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }

    public TournamentStatus Status { get; set; }

    public ICollection<TournamentRegistration> Registrations { get; set; }
        = new List<TournamentRegistration>();

    public ICollection<Match> Matches { get; set; }
        = new List<Match>();

    public ICollection<TournamentStanding> Standings { get; set; }
        = new List<TournamentStanding>();
}