using CommerceCore.Shared.Entities;
using CommerceCore.Domain.Entities.Orders;
namespace CommerceCore.Domain.Entities.Catalog;

/// <summary>
/// The core, deliberately generic product entity. It carries only fields that
/// are true of every product in every vertical (perfume, bedsheets, electronics,
/// jewelry, ...). Anything vertical-specific (Volume, Thread Count, RAM, Fragrance
/// Notes, Gender, ...) is expressed through <see cref="ProductAttribute"/> rows
/// against store-defined <see cref="AttributeDefinition"/>s — never as a column here.
/// </summary>
public class Product : BaseEntity, IStoreScoped
{
    public Guid StoreId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }

    /// <summary>Base SKU. If the product has variants, each variant carries its own SKU
    /// and this becomes a parent/reference SKU rather than a purchasable one.</summary>
    public string? Sku { get; set; }

    public decimal BasePrice { get; set; }
    public decimal? CompareAtPrice { get; set; }   // "was" price, for showing a discount
    public decimal? CostPrice { get; set; }         // internal cost, for margin reporting

    public bool HasVariants { get; set; }
    public bool TrackInventory { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }

    public decimal? WeightKg { get; set; }
    public decimal? LengthCm { get; set; }
    public decimal? WidthCm { get; set; }
    public decimal? HeightCm { get; set; }

    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }

    public Guid? BrandId { get; set; }
    public Brand? Brand { get; set; }

    public Guid? TaxId { get; set; }               // FK into reference Taxes table (later batch)

    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }

    // SEO
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }

    public DateTime? PublishedAt { get; set; }

    // Navigation
    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<ProductAttribute> AttributeAssignments { get; set; } = new List<ProductAttribute>();
    public ICollection<CollectionProduct> CollectionProducts { get; set; } = new List<CollectionProduct>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
