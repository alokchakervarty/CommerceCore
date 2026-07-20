using CommerceCore.Shared.Entities;

namespace CommerceCore.Domain.Entities.Cms;

/// <summary>A static, store-authored page — "About Us", "Terms of Service", "Shipping
/// Policy", "Size Guide", etc. Generic enough that every vertical uses the same table.</summary>
public class Page : BaseEntity, IStoreScoped
{
    public Guid StoreId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;   // rich HTML/Markdown content

    public bool IsPublished { get; set; } = true;

    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
}
