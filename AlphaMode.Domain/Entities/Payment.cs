using AlphaMode.Domain.Common;
using AlphaMode.Domain.Enums;

namespace AlphaMode.Domain.Entities;
public class Payment : AuditableEntity
{
    public Guid PlayerId { get; set; }

    public Player Player { get; set; } = null!;

    public Guid? TournamentId { get; set; }

    public Tournament? Tournament { get; set; }

    public Guid? TournamentRegistrationId { get; set; }
    public TournamentRegistration? TournamentRegistration { get; set; }
    public decimal Amount { get; set; }

    public string Currency { get; set; } = "NGN";

    public PaymentStatus Status { get; set; }

    public PaymentType Type { get; set; }

    public PaymentMethod Method { get; set; }

    public string TransactionReference { get; set; } = string.Empty;

    public string? ProviderReference { get; set; }

    public string? PaymentProvider { get; set; }

    public DateTime? PaidAt { get; set; }
}