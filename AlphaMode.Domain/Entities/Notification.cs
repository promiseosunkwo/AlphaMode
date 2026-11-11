using AlphaMode.Domain.Common;
using AlphaMode.Domain.Identity;

namespace AlphaMode.Domain.Entities;
public class Notification : AuditableEntity
{
    public string UserId { get; set; } = string.Empty;

    public ApplicationUser User { get; set; } = null!;

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public DateTime? ReadAt { get; set; }
}