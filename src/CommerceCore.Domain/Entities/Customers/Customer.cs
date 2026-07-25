using CommerceCore.Domain.Entities.Identity;
using CommerceCore.Domain.Entities.Orders;
using CommerceCore.Shared.Entities;

namespace CommerceCore.Domain.Entities.Customers;

/// <summary>
/// A storefront shopper, scoped to one Store. Deliberately separate from
/// <see cref="User"/>: a Customer can check out as a guest (UserId is null),
/// and later create an account which links UserId back to an Identity User for
/// login. Admins/staff (Identity.User) never appear as Customers, and vice versa.
/// </summary>
public class Customer : BaseEntity, IStoreScoped
{
    public Guid StoreId { get; set; }

    /// <summary>Set once the shopper registers/logs in; null for pure guest checkouts.</summary>
    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }

    public bool IsGuest { get; set; } = true;
    public bool AcceptsMarketing { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>Free-text internal CRM note, never shown to the shopper.</summary>
    public string? InternalNotes { get; set; }

    // Denormalized rollups, maintained by order-completion handlers for fast admin listing
    public int TotalOrdersCount { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTime? LastOrderDate { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();

    public ICollection<Address> Addresses { get; set; } = new List<Address>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    // No CartItems collection here on purpose: CartItem.CustomerId is nullable
    // (guest carts use GuestId instead), and CartItemConfiguration wires that
    // relationship as HasOne(...).WithMany() — no inverse collection. Adding one
    // back here without also updating that WithMany() call to WithMany(c =>
    // c.CartItems) causes EF Core to treat it as a second, separate relationship
    // and invent a phantom shadow FK column ("CustomerId1") for it. Query a
    // customer's cart via db.CartItems.Where(...) instead (see CartMapper).
}
