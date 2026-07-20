using CommerceCore.Domain.Entities.Orders;
using CommerceCore.Domain.Enums;
using CommerceCore.Shared.Entities;

namespace CommerceCore.Domain.Entities.Payments;

/// <summary>
/// One checkout payment attempt against an Order. An Order can have more than one
/// Payment over its life (e.g. an initial failed attempt, then a successful retry).
/// The granular gateway-level history (authorize/capture/refund/void calls and their
/// raw responses) lives in <see cref="PaymentTransaction"/> rows underneath it.
/// </summary>
public class Payment : BaseEntity, IStoreScoped
{
    public Guid StoreId { get; set; }

    public Guid OrderId { get; set; }
    public Order? Order { get; set; }

    public string Provider { get; set; } = string.Empty;   // "Stripe", "Razorpay", "PayPal", ...
    public PaymentMethodType MethodType { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "USD";

    public decimal AmountCaptured { get; set; }
    public decimal AmountRefunded { get; set; }

    public string? GatewayCustomerId { get; set; }
    public string? GatewayPaymentIntentId { get; set; }   // e.g. Stripe PaymentIntent id, Razorpay order id

    public DateTime? AuthorizedAt { get; set; }
    public DateTime? CapturedAt { get; set; }
    public DateTime? FailedAt { get; set; }
    public string? FailureReason { get; set; }

    public ICollection<PaymentTransaction> Transactions { get; set; } = new List<PaymentTransaction>();
}

/// <summary>
/// An immutable, append-only ledger row for a single gateway operation (authorize,
/// capture, refund, void, chargeback) against a Payment. Never updated after creation —
/// the Payment's aggregate fields (Status, AmountCaptured, AmountRefunded) are what get
/// recalculated as new transactions arrive, giving a full auditable gateway history.
/// </summary>
public class PaymentTransaction : BaseEntity
{
    public Guid PaymentId { get; set; }
    public Payment? Payment { get; set; }

    public PaymentTransactionType Type { get; set; }
    public PaymentTransactionStatus Status { get; set; }

    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "USD";

    public string? GatewayTransactionId { get; set; }

    /// <summary>Raw JSON payload returned by the gateway for this operation, kept for
    /// dispute resolution and debugging. Never parsed for business logic — only the
    /// structured fields above are.</summary>
    public string? GatewayResponseRaw { get; set; }

    public string? FailureReason { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}
