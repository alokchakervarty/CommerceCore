namespace CommerceCore.Contracts.Billing;

public record InvoiceItemDto(
    string ProductName,
    string? VariantDisplayName,
    string Sku,
    string? HsnCode,
    decimal UnitPrice,
    int Quantity,
    decimal TaxableValue,
    decimal GstRatePercentage,
    decimal CgstAmount,
    decimal SgstAmount,
    decimal IgstAmount,
    decimal LineTotal);

public record InvoiceDto(
    Guid Id,
    string InvoiceNumber,
    string FinancialYear,
    DateTime InvoiceDate,

    string SellerLegalName,
    string? SellerGstNumber,
    string? SellerPanNumber,
    string? SellerAddressLine1,
    string? SellerAddressLine2,
    string? SellerCity,
    string? SellerState,
    string? SellerPostalCode,

    string BuyerName,
    string? BuyerAddressLine1,
    string? BuyerAddressLine2,
    string? BuyerCity,
    string? BuyerState,
    string? BuyerPostalCode,
    string? BuyerPhoneNumber,

    bool IsInterStateSupply,
    string PlaceOfSupplyState,

    decimal TaxableValue,
    decimal TotalCgstAmount,
    decimal TotalSgstAmount,
    decimal TotalIgstAmount,
    decimal TotalDiscountAmount,
    decimal TotalAmount,

    IReadOnlyList<InvoiceItemDto> Items);
