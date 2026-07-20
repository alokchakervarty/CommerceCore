using CommerceCore.Shared.Entities;

namespace CommerceCore.Domain.Entities.Cms;

/// <summary>A promotional banner/slide for the storefront homepage or category pages —
/// generic enough for a hero carousel in any vertical.</summary>
public class Banner : BaseEntity, IStoreScoped
{
    public Guid StoreId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? MobileImageUrl { get; set; }

    public string? LinkType { get; set; }     // "Category", "Product", "Collection", "Page", "External"
    public string? LinkTarget { get; set; }

    public string Placement { get; set; } = "homepage-hero";  // matched by the theme/frontend

    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
}

/// <summary>A store FAQ entry. Optionally grouped by a free-text Category label
/// (kept simple — a full FaqCategory table wasn't in scope).</summary>
public class Faq : BaseEntity, IStoreScoped
{
    public Guid StoreId { get; set; }

    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string? Category { get; set; }   // e.g. "Shipping", "Returns", "Payments"

    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
