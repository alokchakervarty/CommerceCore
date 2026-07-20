using CommerceCore.Shared.Entities;

namespace CommerceCore.Domain.Entities.Reference;

/// <summary>A store-defined tax rate, optionally scoped to a specific country/state
/// (e.g. US sales tax varies by state; VAT is usually country-wide).</summary>
public class Tax : BaseEntity, IStoreScoped
{
    public Guid StoreId { get; set; }

    public string Name { get; set; } = string.Empty;    // "VAT", "GST", "NY State Sales Tax"
    public decimal RatePercentage { get; set; }

    public Guid? CountryId { get; set; }
    public Guid? StateId { get; set; }

    public bool IsActive { get; set; } = true;
    public int Priority { get; set; }   // when multiple taxes could apply, lower runs first
}
