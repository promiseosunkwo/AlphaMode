using AlphaMode.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphaMode.Application.Payments.DTOs
{
    public class PaymentDetailsDto
    {
        public Guid Id { get; set; }

        public Guid PlayerId { get; set; }
        public string GamerTag { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }

        public Guid? TournamentId { get; set; }
        public string? TournamentName { get; set; }
        public string? GameName { get; set; }

        public Guid? TournamentRegistrationId { get; set; }

        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;

        public PaymentStatus Status { get; set; }
        public PaymentType Type { get; set; }
        public PaymentMethod Method { get; set; }

        public string TransactionReference { get; set; } = string.Empty;
        public string? ProviderReference { get; set; }
        public string? PaymentProvider { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }

        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}
