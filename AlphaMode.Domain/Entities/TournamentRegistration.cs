using AlphaMode.Domain.Common;
using AlphaMode.Domain.Enums;

namespace AlphaMode.Domain.Entities;
public class TournamentRegistration : AuditableEntity
{
    public Guid TournamentId { get; set; }

    public Tournament Tournament { get; set; } = null!;

    public Guid PlayerId { get; set; }

    public Player Player { get; set; } = null!;

    public TournamentRegistrationStatus Status { get; set; }

    public DateTime? CheckedInAt { get; set; }

    public DateTime? DisqualifiedAt { get; set; }

    public string? DisqualificationReason { get; set; }

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}