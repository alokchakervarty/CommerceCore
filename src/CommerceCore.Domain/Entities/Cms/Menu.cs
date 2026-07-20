using CommerceCore.Shared.Entities;

namespace CommerceCore.Domain.Entities.Cms;

/// <summary>A named navigation menu for a store's storefront, e.g. "Main Header Menu",
/// "Footer Menu". A store can define as many as its theme needs.</summary>
public class Menu : BaseEntity, IStoreScoped
{
    public Guid StoreId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;  // "header", "footer", "sidebar" — matched by the theme
    public bool IsActive { get; set; } = true;

    public ICollection<MenuItem> Items { get; set; } = new List<MenuItem>();
}

/// <summary>
/// A single navigation link within a Menu. Self-referencing for dropdown/nested menus.
/// LinkType + LinkTarget together describe where it points (a category, a product,
/// a CMS page, an external URL, ...) without needing a separate table per link kind.
/// </summary>
public class MenuItem : BaseEntity
{
    public Guid MenuId { get; set; }
    public Menu? Menu { get; set; }

    public Guid? ParentMenuItemId { get; set; }
    public MenuItem? ParentMenuItem { get; set; }
    public ICollection<MenuItem> Children { get; set; } = new List<MenuItem>();

    public string Label { get; set; } = string.Empty;

    /// <summary>"Category", "Product", "Collection", "Page", "External"</summary>
    public string LinkType { get; set; } = "External";

    /// <summary>The target Id (as string, since it could reference different entity types)
    /// when LinkType is an internal reference, or the raw URL when LinkType is "External".</summary>
    public string LinkTarget { get; set; } = string.Empty;

    public bool OpenInNewTab { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
