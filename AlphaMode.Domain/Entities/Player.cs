using AlphaMode.Domain.Common;
using AlphaMode.Domain.Identity;

namespace AlphaMode.Domain.Entities;

public class Player : AuditableEntity
{
    public string UserId { get; set; } = string.Empty;

    public ApplicationUser User { get; set; } = null!;

    public string GamerTag { get; set; } = string.Empty;

    public int RankingPoints { get; set; }

    public int Wins { get; set; }

    public int Losses { get; set; }

    public int Draws { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<TournamentRegistration> Registrations { get; set; } = new List<TournamentRegistration>();
}