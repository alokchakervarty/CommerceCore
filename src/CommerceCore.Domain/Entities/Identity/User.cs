using CommerceCore.Shared.Entities;

namespace CommerceCore.Domain.Entities.Identity;

/// <summary>
/// An authenticatable principal: admins, store staff, or API consumers.
/// Storefront shoppers are represented by <see cref="Customers.Customer"/>, which
/// optionally links back to a User when the shopper creates an account.
/// </summary>
public class User : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsLockedOut { get; set; }
    public DateTime? LockoutEndDate { get; set; }
    public int AccessFailedCount { get; set; }
    public DateTime? LastLoginDate { get; set; }

    /// <summary>Null for platform-level (super admin) users; set for store-scoped staff.</summary>
    public Guid? StoreId { get; set; }

    // Navigation
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public string FullName => $"{FirstName} {LastName}".Trim();
}
