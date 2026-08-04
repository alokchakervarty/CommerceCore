using CommerceCore.Domain.Enums;

namespace CommerceCore.Api.Models;

public class AddressRequest
{
 public string? Type { get; set; }
 public string FullName { get; set; } = string.Empty;
 public string PhoneNumber { get; set; } = string.Empty;
 public string AddressLine1 { get; set; } = string.Empty;
 public string? AddressLine2 { get; set; }
 public string City { get; set; } = string.Empty;
 public string State { get; set; } = string.Empty;
 public string PostalCode { get; set; } = string.Empty;
 public Guid? CountryId { get; set; }
 public bool IsDefaultShipping { get; set; }
 public bool IsDefaultBilling { get; set; }
}
