using CommerceCore.Shared.Entities;

namespace CommerceCore.Domain.Entities.Reference;

public class Currency : BaseEntity
{
    public string Code { get; set; } = string.Empty;    // "USD", "INR"
    public string Name { get; set; } = string.Empty;     // "US Dollar"
    public string Symbol { get; set; } = string.Empty;   // "$"
    public int DecimalPlaces { get; set; } = 2;
    public decimal ExchangeRateToBase { get; set; } = 1;  // relative to a platform base currency
    public bool IsActive { get; set; } = true;
}

public class Language : BaseEntity
{
    public string Code { get; set; } = string.Empty;    // "en", "hi"
    public string Name { get; set; } = string.Empty;      // "English"
    public string NativeName { get; set; } = string.Empty; // "English", "हिन्दी"
    public bool IsRtl { get; set; }
    public bool IsActive { get; set; } = true;
}
