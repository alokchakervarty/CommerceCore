using CommerceCore.Domain.Entities.Catalog;
using CommerceCore.Domain.Entities.Customers;
using CommerceCore.Domain.Entities.Orders;
using CommerceCore.Shared.Entities;

namespace CommerceCore.Domain.Entities.Reviews;

/// <summary>A customer's rating/review of a Product. Links back to the Order that
/// proves a verified purchase (nullable — reviews can be allowed without a purchase
/// depending on store policy, decided in the Application layer, not the schema).</summary>
public class Review : BaseEntity, IStoreScoped
{
    public Guid StoreId { get; set; }

    public Guid ProductId { get; set; }
    public Product? Product { get; set; }

    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public Guid? OrderId { get; set; }
    public Order? Order { get; set; }

    public int Rating { get; set; }   // 1-5, enforced by a DB check constraint
    public string? Title { get; set; }
    public string? Body { get; set; }

    public bool IsVerifiedPurchase { get; set; }

    /// <summary>Moderation gate: reviews don't appear on the storefront until approved.</summary>
    public bool IsApproved { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? ApprovedByUserId { get; set; }

    public int HelpfulCount { get; set; }

    public string? MerchantReplyBody { get; set; }
    public DateTime? MerchantRepliedAt { get; set; }
}
