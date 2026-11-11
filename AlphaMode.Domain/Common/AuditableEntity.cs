namespace AlphaMode.Domain.Common;

public abstract class AuditableEntity : BaseEntity
{
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime? ModifiedDate { get; set; }

    public string? CreatedBy { get; set; }

    public string? ModifiedBy { get; set; }
}