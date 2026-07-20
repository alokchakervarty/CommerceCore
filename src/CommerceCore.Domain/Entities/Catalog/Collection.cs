using CommerceCore.Shared.Entities;

namespace CommerceCore.Domain.Entities.Catalog;

/// <summary>A curated, cross-category grouping of products, e.g. "Summer Sale",
/// "New Arrivals", "Editor's Picks". Independent of the Category tree.</summary>
public class Collection : BaseEntity, IStoreScoped
{
    public Guid StoreId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>Optional scheduling window, e.g. a seasonal sale collection.</summary>
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }

    public ICollection<CollectionProduct> CollectionProducts { get; set; } = new List<CollectionProduct>();
}

/// <summary>Many-to-many join between Collection and Product, with per-item ordering.</summary>
public class CollectionProduct : BaseEntity
{
    public Guid CollectionId { get; set; }
    public Collection? Collection { get; set; }

    public Guid ProductId { get; set; }
    public Product? Product { get; set; }

    public int DisplayOrder { get; set; }
}
