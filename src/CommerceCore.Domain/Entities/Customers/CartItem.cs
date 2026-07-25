using CommerceCore.Domain.Entities.Catalog;
using CommerceCore.Shared.Entities;

namespace CommerceCore.Domain.Entities.Customers;

/// <summary>
/// A line in a shopping cart, prior to checkout. Belongs to exactly one of a
/// registered Customer (CustomerId) or an anonymous guest (GuestId), never both —
/// carts can be built before login, and are merged into the Customer's cart
/// automatically the moment they log in, register, or complete OTP login (see
/// GuestCartMerger in the Application layer). Not present in the original table
/// list, but added because a working "add to cart, then check out" flow needs
/// somewhere to hold selections before an Order exists — Orders/OrderItems are
/// snapshots taken AT checkout, not a place to stage in-progress selections.
/// Cleared once its Customer completes checkout (see Order creation).
/// </summary>
public class CartItem : BaseEntity
{
    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    /// <summary>Client-generated identifier for an anonymous cart, sent via the
    /// X-Guest-Id header. Set instead of CustomerId while the shopper isn't logged in.</summary>
    public Guid? GuestId { get; set; }

    public Guid ProductId { get; set; }
    public Product? Product { get; set; }

    public Guid ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }

    public int Quantity { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
