using CommerceCore.Domain.Entities.Customers;
using CommerceCore.Domain.Enums;
using CommerceCore.Shared.Entities;

namespace CommerceCore.Domain.Entities.Orders;

/// <summary>
/// A placed order. Shipping/billing details, and every OrderItem's product name/SKU/price,
/// are snapshotted at checkout time rather than referencing the live Product/Address —
/// so an order always reflects exactly what the customer bought and paid, even if the
/// product is later renamed/repriced/deleted or the address edited.
/// </summary>
public class Order : BaseEntity, IStoreScoped
{
    public Guid StoreId { get; set; }

    /// <summary>Human-facing order number (e.g. "ST1-100042"), distinct from the internal Guid Id.</summary>
    public string OrderNumber { get; set; } = string.Empty;

    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public OrderPaymentStatus PaymentStatus { get; set; } = OrderPaymentStatus.Pending;

    public string CurrencyCode { get; set; } = "USD";

    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }

    public Guid? CouponId { get; set; }
    public string? CouponCode { get; set; }   // snapshotted so history reads correctly if the coupon is later deleted

    public Guid? ShippingMethodId { get; set; }
    public string? ShippingMethodName { get; set; }  // snapshot

    // Shipping address snapshot
    public string ShippingFullName { get; set; } = string.Empty;
    public string ShippingPhoneNumber { get; set; } = string.Empty;
    public string ShippingAddressLine1 { get; set; } = string.Empty;
    public string? ShippingAddressLine2 { get; set; }
    public string ShippingCity { get; set; } = string.Empty;
    public string ShippingState { get; set; } = string.Empty;
    public string ShippingPostalCode { get; set; } = string.Empty;
    public string ShippingCountry { get; set; } = string.Empty;

    // Billing address snapshot
    public string BillingFullName { get; set; } = string.Empty;
    public string BillingPhoneNumber { get; set; } = string.Empty;
    public string BillingAddressLine1 { get; set; } = string.Empty;
    public string? BillingAddressLine2 { get; set; }
    public string BillingCity { get; set; } = string.Empty;
    public string BillingState { get; set; } = string.Empty;
    public string BillingPostalCode { get; set; } = string.Empty;
    public string BillingCountry { get; set; } = string.Empty;

    public string? CustomerNote { get; set; }   // left by the shopper at checkout
    public string? InternalNote { get; set; }    // staff-only

    public DateTime PlacedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancelReason { get; set; }

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public string? PaymentMethod { get; set; }
}
