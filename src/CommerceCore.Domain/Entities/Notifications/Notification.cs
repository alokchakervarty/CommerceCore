using CommerceCore.Domain.Enums;
using CommerceCore.Shared.Entities;

namespace CommerceCore.Domain.Entities.Notifications;

/// <summary>A reusable, store-editable message template (order confirmation, shipment
/// update, back-in-stock alert, ...) with placeholder tokens like {{customerName}},
/// {{orderNumber}} substituted when a Notification is actually sent.</summary>
public class NotificationTemplate : BaseEntity, IStoreScoped
{
    public Guid StoreId { get; set; }

    public string Code { get; set; } = string.Empty;    // "order.confirmed", "product.back_in_stock"
    public string Name { get; set; } = string.Empty;
    public NotificationChannel Channel { get; set; }

    public string? SubjectTemplate { get; set; }   // used for Email
    public string BodyTemplate { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

/// <summary>A single notification instance sent (or queued to send) to a recipient —
/// the delivery log, distinct from the reusable template it was rendered from.</summary>
public class Notification : BaseEntity, IStoreScoped
{
    public Guid StoreId { get; set; }

    public Guid? NotificationTemplateId { get; set; }
    public NotificationTemplate? NotificationTemplate { get; set; }

    public NotificationChannel Channel { get; set; }

    /// <summary>Polymorphic recipient reference — usually a Customer or User Id.</summary>
    public Guid RecipientId { get; set; }
    public string RecipientAddress { get; set; } = string.Empty;   // email address / phone number

    public string? Subject { get; set; }
    public string Body { get; set; } = string.Empty;   // fully rendered, final content actually sent

    public bool IsSent { get; set; }
    public DateTime? SentAt { get; set; }
    public bool IsRead { get; set; }         // relevant for InApp
    public DateTime? ReadAt { get; set; }

    public string? FailureReason { get; set; }
    public int RetryCount { get; set; }
}
