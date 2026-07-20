using CommerceCore.Domain.Entities.Catalog;
using CommerceCore.Domain.Entities.Inventory;
using CommerceCore.Shared.Entities;

namespace CommerceCore.Domain.Entities.Orders;

/// <summary>
/// A single line item on an Order. ProductId/ProductVariantId are kept (nullable-safe
/// on delete via SetNull, configured in Infrastructure) purely for reporting/navigation;
/// every field the customer actually saw at checkout — name, variant label, SKU, price —
/// is snapshotted directly onto this row so order history never changes retroactively.
/// </summary>
public class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order? Order { get; set; }

    public Guid? ProductId { get; set; }
    public Product? Product { get; set; }

    public Guid? ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }

    /// <summary>Which warehouse this line was (or will be) fulfilled from.</summary>
    public Guid? WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }

    public string ProductNameSnapshot { get; set; } = string.Empty;
    public string? VariantDisplayNameSnapshot { get; set; }   // e.g. "50ml / Red"
    public string SkuSnapshot { get; set; } = string.Empty;
    public string? ImageUrlSnapshot { get; set; }

    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }

    public decimal LineTotal => (UnitPrice * Quantity) - DiscountAmount + TaxAmount;
}
