using CommerceCore.Domain.Enums;
using CommerceCore.Shared.Entities;

namespace CommerceCore.Domain.Entities.Marketing;

/// <summary>A store-defined discount code. Eligibility scoping (specific products/
/// categories/collections vs. store-wide) is deliberately kept generic here — the
/// optional CategoryId/ProductId/CollectionId fields let a coupon target one of them,
/// or none (store-wide), without introducing extra join tables the spec didn't ask for.</summary>
public class Coupon : BaseEntity, IStoreScoped
{
    public Guid StoreId { get; set; }

    public string Code { get; set; } = string.Empty;     // uppercased, unique per store
    public string? Description { get; set; }

    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }              // percentage (0-100) or fixed amount, per DiscountType

    public decimal? MinimumOrderAmount { get; set; }
    public decimal? MaxDiscountAmount { get; set; }          // caps a percentage discount

    /// <summary>Null = unlimited redemptions overall / per customer.</summary>
    public int? UsageLimitTotal { get; set; }
    public int? UsageLimitPerCustomer { get; set; }
    public int TimesUsed { get; set; }

    public Guid? RestrictedToCategoryId { get; set; }
    public Guid? RestrictedToProductId { get; set; }
    public Guid? RestrictedToCollectionId { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }

    public ICollection<CouponUsage> Usages { get; set; } = new List<CouponUsage>();
}

/// <summary>One redemption of a Coupon against a specific Order. Enforces (via a unique
/// index in Infrastructure) that the same coupon+customer combination cannot be counted
/// twice for a single order, and gives per-customer usage-limit enforcement a real table
/// to query instead of recomputing from Orders.</summary>
public class CouponUsage : BaseEntity
{
    public Guid CouponId { get; set; }
    public Coupon? Coupon { get; set; }

    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }

    public decimal DiscountAmountApplied { get; set; }
    public DateTime UsedAt { get; set; } = DateTime.UtcNow;
}
