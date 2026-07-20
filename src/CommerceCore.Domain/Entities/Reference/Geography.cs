using CommerceCore.Shared.Entities;

namespace CommerceCore.Domain.Entities.Reference;

/// <summary>Platform-wide reference data — not store-scoped, since geography is
/// shared by every tenant. Seeded once at deployment time (see database/seed scripts).</summary>
public class Country : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Iso2Code { get; set; } = string.Empty;   // "US", "IN"
    public string Iso3Code { get; set; } = string.Empty;    // "USA", "IND"
    public string? PhoneCode { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<State> States { get; set; } = new List<State>();
}

public class State : BaseEntity
{
    public Guid CountryId { get; set; }
    public Country? Country { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<City> Cities { get; set; } = new List<City>();
}

public class City : BaseEntity
{
    public Guid StateId { get; set; }
    public State? State { get; set; }

    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
