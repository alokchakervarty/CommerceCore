using CommerceCore.Contracts.Billing;
using CommerceCore.Domain.Entities.Billing;
using CommerceCore.Domain.Entities.Orders;

namespace CommerceCore.Application.Features.Billing;

internal static class InvoiceMapper
{
    public static InvoiceDto ToDto(Invoice invoice, IReadOnlyList<OrderItem> orderItems) => new(
        invoice.Id,
        invoice.InvoiceNumber,
        invoice.FinancialYear,
        invoice.InvoiceDate,

        invoice.SellerLegalName,
        invoice.SellerGstNumber,
        invoice.SellerPanNumber,
        invoice.SellerAddressLine1,
        invoice.SellerAddressLine2,
        invoice.SellerCity,
        invoice.SellerState,
        invoice.SellerPostalCode,

        invoice.BuyerName,
        invoice.BuyerAddressLine1,
        invoice.BuyerAddressLine2,
        invoice.BuyerCity,
        invoice.BuyerState,
        invoice.BuyerPostalCode,
        invoice.BuyerPhoneNumber,

        invoice.IsInterStateSupply,
        invoice.PlaceOfSupplyState,

        invoice.TaxableValue,
        invoice.TotalCgstAmount,
        invoice.TotalSgstAmount,
        invoice.TotalIgstAmount,
        invoice.TotalDiscountAmount,
        invoice.TotalAmount,

        orderItems.Select(oi => new InvoiceItemDto(
            oi.ProductNameSnapshot,
            oi.VariantDisplayNameSnapshot,
            oi.SkuSnapshot,
            oi.HsnCodeSnapshot,
            oi.UnitPrice,
            oi.Quantity,
            oi.UnitPrice * oi.Quantity,
            oi.GstRatePercentageSnapshot,
            oi.CgstAmount,
            oi.SgstAmount,
            oi.IgstAmount,
            oi.LineTotal)).ToList());
}
