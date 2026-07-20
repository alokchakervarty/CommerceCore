using CommerceCore.Domain.Enums;
using CommerceCore.Shared.Entities;

namespace CommerceCore.Domain.Entities.Reference;

/// <summary>A named group of destination regions a store ships to, e.g. "Domestic",
/// "EU", "Rest of World". Scoping is expressed via optional Country/State — leaving
/// both null means "everywhere" (a catch-all zone).</summary>
public class ShippingZone : BaseEntity, IStoreScoped
{
    public Guid StoreId { get; set; }

    public string Name { get; set; } = string.Empty;
    public Guid? CountryId { get; set; }
    public Guid? StateId { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<ShippingMethod> ShippingMethods { get; set; } = new List<ShippingMethod>();
}

/// <summary>A purchasable shipping option within a ShippingZone, e.g. "Standard (5-7 days)"
/// or "Express (1-2 days)", each with its own rate calculation strategy.</summary>
public class ShippingMethod : BaseEntity, IStoreScoped
{
    public Guid StoreId { get; set; }

    public Guid ShippingZoneId { get; set; }
    public ShippingZone? ShippingZone { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ShippingRateType RateType { get; set; } = ShippingRateType.Flat;
    public decimal FlatRate { get; set; }
    public decimal? RatePerKg { get; set; }              // used when RateType == WeightBased
    public decimal? FreeShippingThreshold { get; set; }   // used when RateType == PriceBased

    public int EstimatedDaysMin { get; set; }
    public int EstimatedDaysMax { get; set; }

    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
}
