namespace CommerceCore.Domain.Enums;

public enum PaymentMethodType
{
    CreditCard = 0,
    DebitCard = 1,
    Upi = 2,
    NetBanking = 3,
    Wallet = 4,
    CashOnDelivery = 5,
    BankTransfer = 6
}

/// <summary>Overall status of a Payment (one per checkout attempt against an Order).</summary>
public enum PaymentStatus
{
    Pending = 0,
    Authorized = 1,
    Captured = 2,
    PartiallyCaptured = 3,
    Failed = 4,
    Voided = 5,
    Refunded = 6,
    PartiallyRefunded = 7
}

/// <summary>The kind of gateway operation a single PaymentTransaction ledger row represents.</summary>
public enum PaymentTransactionType
{
    Authorization = 0,
    Capture = 1,
    Refund = 2,
    Void = 3,
    Chargeback = 4
}

public enum PaymentTransactionStatus
{
    Pending = 0,
    Success = 1,
    Failed = 2
}
