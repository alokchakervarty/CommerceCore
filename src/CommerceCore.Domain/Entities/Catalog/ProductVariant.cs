using CommerceCore.Shared.Entities;

namespace CommerceCore.Domain.Entities.Catalog;

/// <summary>
/// A specific purchasable combination of a product's variant-dimension attributes,
/// e.g. "Midnight Oud, 50ml, Red Bottle". When Product.HasVariants is false, the
/// product still gets exactly one implicit "default" variant so that pricing and
/// stock (managed in the Inventory batch) always live at the variant level uniformly.
/// </summary>
public class ProductVariant : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }

    public string Sku { get; set; } = string.Empty;
    public string? Barcode { get; set; }

    /// <summary>Overrides Product.BasePrice when set; null means "inherit from product".</summary>
    public decimal? Price { get; set; }
    public decimal? CompareAtPrice { get; set; }

    public decimal? WeightKg { get; set; }
    public string? ImageUrl { get; set; }   // variant-specific hero image, e.g. the red bottle

    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; }      // the implicit variant for non-variant products

    /// <summary>Human-readable label built from its attribute assignments, e.g. "50ml / Red",
    /// denormalized here for fast list rendering without joining AttributeValues every time.</summary>
    public string DisplayName { get; set; } = string.Empty;

    public ICollection<ProductAttribute> AttributeAssignments { get; set; } = new List<ProductAttribute>();
}
