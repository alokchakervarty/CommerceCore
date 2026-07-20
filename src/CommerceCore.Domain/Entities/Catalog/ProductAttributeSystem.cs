using CommerceCore.Domain.Enums;
using CommerceCore.Shared.Entities;

namespace CommerceCore.Domain.Entities.Catalog;

/// <summary>
/// A store-defined product attribute, e.g. "Volume" (perfume), "Thread Count" (bedsheets),
/// "RAM" (electronics), "Fragrance Notes" (perfume). Maps to the "Attributes" table.
/// This single table is what lets one backend serve every vertical: verticals differ only
/// in which AttributeDefinitions their store creates, never in code.
/// Named "AttributeDefinition" (not "Attribute") to avoid colliding with System.Attribute.
/// </summary>
public class AttributeDefinition : BaseEntity, IStoreScoped
{
    public Guid StoreId { get; set; }

    public string Name { get; set; } = string.Empty;      // "Volume"
    public string Code { get; set; } = string.Empty;       // "volume" (slug, unique per store)
    public AttributeInputType InputType { get; set; } = AttributeInputType.Text;

    /// <summary>When true, this attribute defines purchasable variants (e.g. Size, Color)
    /// rather than being purely descriptive (e.g. Fragrance Notes).</summary>
    public bool IsVariantDimension { get; set; }

    public bool IsFilterable { get; set; }     // shown in storefront filter sidebar
    public bool IsRequired { get; set; }
    public int DisplayOrder { get; set; }

    public ICollection<AttributeValue> Values { get; set; } = new List<AttributeValue>();
}

/// <summary>A predefined value for a Select/MultiSelect/Color attribute, e.g. "50ml", "Red".
/// Maps to the "AttributeValues" table. Text/Number/Boolean/Date attributes don't use
/// predefined values — the raw value is stored directly on the ProductAttribute assignment row.</summary>
public class AttributeValue : BaseEntity
{
    public Guid AttributeId { get; set; }
    public AttributeDefinition? Attribute { get; set; }

    public string Value { get; set; } = string.Empty;   // "50ml", "Red"
    public string? ColorHex { get; set; }                 // only meaningful for Color input type
    public int DisplayOrder { get; set; }
}

/// <summary>
/// Assigns an attribute + value to either a Product (descriptive spec, e.g. "Fragrance
/// Notes: Oud, Amber") or a ProductVariant (purchasing dimension, e.g. "Volume: 50ml" /
/// "Color: Red"). Maps to the "ProductAttributes" table. Exactly one of ProductId /
/// ProductVariantId is set per row, enforced by a DB check constraint.
/// </summary>
public class ProductAttribute : BaseEntity
{
    public Guid? ProductId { get; set; }
    public Product? Product { get; set; }

    public Guid? ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }

    public Guid AttributeId { get; set; }
    public AttributeDefinition? Attribute { get; set; }

    public Guid? AttributeValueId { get; set; }           // set when Attribute uses a predefined value list
    public AttributeValue? AttributeValueRef { get; set; }

    public string? FreeTextValue { get; set; }             // set when Attribute has no predefined value list
}
