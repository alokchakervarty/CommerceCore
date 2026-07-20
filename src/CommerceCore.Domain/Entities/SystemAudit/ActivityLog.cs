namespace CommerceCore.Domain.Entities.SystemAudit;

/// <summary>
/// A human-readable activity feed entry (e.g. "John placed order #ST1-100042",
/// "Admin updated stock for SKU ABC-123"), shown in admin dashboards. Distinct from
/// AuditLog: AuditLog is a complete before/after technical diff for compliance and
/// rollback investigation; ActivityLog is a curated, readable narrative for humans
/// scanning "what's been happening" — not every AuditLog entry produces one.
/// </summary>
public class ActivityLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid? StoreId { get; set; }
    public Guid? ActorUserId { get; set; }
    public string? ActorDisplayName { get; set; }

    public string ActivityType { get; set; } = string.Empty;   // "OrderPlaced", "StockAdjusted", "ProductPublished"
    public string Description { get; set; } = string.Empty;     // fully rendered, human-readable message

    public string? RelatedEntityName { get; set; }
    public Guid? RelatedEntityId { get; set; }

    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}
