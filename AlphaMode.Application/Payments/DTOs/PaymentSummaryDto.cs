
namespace AlphaMode.Application.Payments.DTOs;

public class PaymentSummaryDto
{
    public decimal CompletedRevenue { get; set; }

    public decimal PendingAmount { get; set; }

    public int CompletedCount { get; set; }

    public int PendingCount { get; set; }

    public int FailedCount { get; set; }

    public int RefundedCount { get; set; }
}
