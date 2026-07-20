using CommerceCore.Domain.Entities.Catalog;
using CommerceCore.Domain.Entities.Customers;
using CommerceCore.Shared.Entities;

namespace CommerceCore.Domain.Entities.Marketing;

/// <summary>A named list of saved products for a Customer. Most storefronts only ever
/// need one default wishlist per customer, but this supports named/multiple lists
/// (e.g. "Birthday Ideas") without any extra tables.</summary>
public class Wishlist : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public string Name { get; set; } = "My Wishlist";
    public bool IsDefault { get; set; } = true;
    public bool IsPublic { get; set; }   // shareable via a public link

    public ICollection<WishlistItem> Items { get; set; } = new List<WishlistItem>();
}

/// <summary>A single saved product (optionally a specific variant, e.g. a chosen
/// color/size) within a Wishlist.</summary>
public class WishlistItem : BaseEntity
{
    public Guid WishlistId { get; set; }
    public Wishlist? Wishlist { get; set; }

    public Guid ProductId { get; set; }
    public Product? Product { get; set; }

    public Guid? ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
