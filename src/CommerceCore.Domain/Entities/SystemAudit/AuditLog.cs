namespace CommerceCore.Domain.Entities.SystemAudit;

/// <summary>
/// An immutable, append-only record of a create/update/delete against any audited
/// entity in the system — captured automatically by a SaveChanges interceptor in
/// Infrastructure, not hand-written per module. Deliberately does NOT derive from
/// BaseEntity: an audit log must never itself be soft-deleted, audited, or carry a
/// mutable concurrency token — it is write-once by definition.
/// </summary>
public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid? StoreId { get; set; }
    public Guid? UserId { get; set; }          // who made the change (null for system/background actions)
    public string? UserDisplayName { get; set; } // snapshotted, survives the user later being deleted

    public string EntityName { get; set; } = string.Empty;   // "Product", "Order", ...
    public Guid EntityId { get; set; }
    public string Action { get; set; } = string.Empty;         // "Created", "Updated", "Deleted"

    public string? OldValuesJson { get; set; }   // serialized snapshot before the change (null on Create)
    public string? NewValuesJson { get; set; }    // serialized snapshot after the change (null on Delete)
    public string? ChangedPropertiesJson { get; set; } // list of property names that actually changed

    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}
