using AlphaMode.Domain.Common;

namespace AlphaMode.Domain.Entities;
public class AuditLog : AuditableEntity
{
    public string Action { get; set; } = string.Empty;

    public string EntityName { get; set; } = string.Empty;

    public string? EntityId { get; set; }

    public string? UserId { get; set; }

    public string? UserName { get; set; }

    public string? Changes { get; set; }
}