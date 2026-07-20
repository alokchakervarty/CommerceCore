using CommerceCore.Domain.Entities.Catalog;
using CommerceCore.Shared.Entities;

namespace CommerceCore.Domain.Entities.Inventory;

/// <summary>
/// Current stock level for one ProductVariant at one Warehouse. Maps to the
/// "Inventory" table. Named "InventoryItem" rather than "Inventory" to avoid a
/// class sharing its name with the containing namespace, which is legal C# but
/// causes needless ambiguity in tooling and code navigation.
/// </summary>
public class InventoryItem : BaseEntity
{
    public Guid WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }

    public Guid ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }

    public int QuantityOnHand { get; set; }
    public int QuantityReserved { get; set; }   // held against unshipped orders

    public int QuantityAvailable => QuantityOnHand - QuantityReserved;

    public int ReorderPoint { get; set; }        // trigger a low-stock alert/reorder below this
    public int ReorderQuantity { get; set; }      // suggested quantity to reorder

    public string? BinLocation { get; set; }      // aisle/shelf/bin within the warehouse

    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
}
