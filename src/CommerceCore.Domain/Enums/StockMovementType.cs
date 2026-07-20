namespace CommerceCore.Domain.Enums;

/// <summary>Classifies every quantity change recorded in StockMovement, giving a
/// full, reconstructable audit trail of why stock moved.</summary>
public enum StockMovementType
{
    PurchaseReceipt = 0,   // stock received from a supplier
    SaleReserved = 1,       // reserved against a placed order (not yet shipped)
    SaleFulfilled = 2,      // released from reserved and deducted on shipment
    SaleCancelled = 3,      // reservation released back to available stock
    ReturnRestock = 4,      // customer return added back to sellable stock
    Adjustment = 5,         // manual correction (stock take, damage found, etc.)
    TransferOut = 6,        // moved out of this warehouse to another
    TransferIn = 7,         // moved into this warehouse from another
    Damaged = 8,             // written off as damaged/unsellable
    InitialStock = 9         // opening balance when a variant/warehouse pair is first created
}
