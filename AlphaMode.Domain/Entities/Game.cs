using AlphaMode.Domain.Common;

namespace AlphaMode.Domain.Entities;
public class Game : AuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public string Platform { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Tournament> Tournaments { get; set; } = new List<Tournament>();

    // Matches now reference their game directly because friendly matches do not have a tournament.
    public ICollection<Match> Matches { get; set; } = new List<Match>();
}

