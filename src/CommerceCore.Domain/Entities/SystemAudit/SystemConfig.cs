using CommerceCore.Shared.Entities;

namespace CommerceCore.Domain.Entities.SystemAudit;

/// <summary>
/// A generic key/value configuration row. StoreSettings (in the Stores batch) covers
/// the well-known, strongly-typed per-store settings a store owner configures; this
/// table instead covers platform-level or ad-hoc flags/toggles that don't warrant a
/// dedicated column — e.g. feature flags, maintenance mode, integration toggles.
/// </summary>
public class SystemSetting : BaseEntity
{
    /// <summary>Null = platform-wide setting; set = overrides for one specific store.</summary>
    public Guid? StoreId { get; set; }

    public string Key { get; set; } = string.Empty;      // "feature.wishlist_sharing_enabled"
    public string Value { get; set; } = string.Empty;     // stored as text; caller parses to the expected type
    public string? Description { get; set; }
}

/// <summary>A store-editable transactional email template — order confirmations,
/// password resets, shipping updates. Kept separate from NotificationTemplate
/// (which is channel-agnostic and used for the actual send pipeline) because email
/// specifically needs a full HTML layout, not just a body string.</summary>
public class EmailTemplate : BaseEntity, IStoreScoped
{
    public Guid StoreId { get; set; }

    public string Code { get; set; } = string.Empty;    // "order.confirmation", "auth.password_reset"
    public string Name { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    public string? PlainTextBody { get; set; }

    public bool IsActive { get; set; } = true;
}
