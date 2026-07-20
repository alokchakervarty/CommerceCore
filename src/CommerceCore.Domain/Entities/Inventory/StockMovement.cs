using CommerceCore.Domain.Enums;
using CommerceCore.Shared.Entities;

namespace CommerceCore.Domain.Entities.Inventory;

/// <summary>
/// An immutable, append-only record of a single stock quantity change against an
/// InventoryItem. QuantityOnHand on InventoryItem is a denormalized running total;
/// this table is the source of truth for how it got there and is never updated or
/// deleted, only inserted — giving a complete, reconstructable stock history.
/// </summary>
public class StockMovement : BaseEntity
{
    public Guid InventoryItemId { get; set; }
    public InventoryItem? InventoryItem { get; set; }

    public StockMovementType MovementType { get; set; }

    /// <summary>Positive for increases (purchase receipt, return, transfer-in),
    /// negative for decreases (sale fulfilled, damage, transfer-out).</summary>
    public int QuantityChange { get; set; }

    public int QuantityOnHandAfter { get; set; }   // snapshot for fast history rendering without recomputation

    /// <summary>Polymorphic reference to whatever business document caused this movement,
    /// e.g. ReferenceType = "Order", ReferenceId = the Order's Id.</summary>
    public string? ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }

    public string? Notes { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}
