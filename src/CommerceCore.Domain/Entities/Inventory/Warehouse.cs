using CommerceCore.Shared.Entities;

namespace CommerceCore.Domain.Entities.Inventory;

/// <summary>A physical or virtual stock-holding location for a Store. A store may
/// have exactly one (a single warehouse) or many (regional fulfillment centers).</summary>
public class Warehouse : BaseEntity, IStoreScoped
{
    public Guid StoreId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;    // short internal code, e.g. "WH-NORTH"
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; }

    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public Guid? CountryId { get; set; }

    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }

    public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
}
