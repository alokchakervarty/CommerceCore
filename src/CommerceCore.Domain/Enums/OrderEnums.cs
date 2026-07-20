namespace CommerceCore.Domain.Enums;

public enum OrderStatus
{
    Pending = 0,       // placed, awaiting confirmation (e.g. payment auth or manual review)
    Confirmed = 1,      // accepted, will be fulfilled
    Processing = 2,     // being picked/packed
    Shipped = 3,
    Delivered = 4,
    Cancelled = 5,
    Refunded = 6,
    PartiallyRefunded = 7,
    OnHold = 8           // e.g. suspected fraud, awaiting manual review
}

/// <summary>Order-level payment summary. The authoritative, itemized record of each
/// attempt/capture/refund lives in Payments/PaymentTransactions (next batch); this is
/// a denormalized rollup for fast display and filtering on the Order itself.</summary>
public enum OrderPaymentStatus
{
    Pending = 0,
    Authorized = 1,
    Paid = 2,
    PartiallyPaid = 3,
    Refunded = 4,
    PartiallyRefunded = 5,
    Failed = 6,
    Voided = 7
}
