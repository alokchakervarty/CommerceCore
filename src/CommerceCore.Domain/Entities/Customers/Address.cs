using CommerceCore.Domain.Enums;
using CommerceCore.Shared.Entities;
using CommerceCore.Domain.Entities.Reference;

namespace CommerceCore.Domain.Entities.Customers;

/// <summary>A saved shipping/billing address belonging to a Customer. Orders don't
/// reference this table directly — they snapshot the full address at checkout time
/// onto the Order itself, so a later edit/deletion here never rewrites order history.</summary>
public class Address : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public AddressType Type { get; set; } = AddressType.Both;

    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;

    // Country is optional at the API level; make the FK nullable so addresses can be saved without it.
    public Guid? CountryId { get; set; }
    public Country? Country { get; set; }

    public bool IsDefaultShipping { get; set; }
    public bool IsDefaultBilling { get; set; }
}
