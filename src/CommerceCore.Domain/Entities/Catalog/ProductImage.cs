using CommerceCore.Shared.Entities;

namespace CommerceCore.Domain.Entities.Catalog;

/// <summary>A gallery image for a Product, optionally scoped to one specific
/// ProductVariant (e.g. showing only the red bottle's photos when Color=Red is selected).</summary>
public class ProductImage : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }

    public Guid? ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }

    public string Url { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsPrimary { get; set; }
}
