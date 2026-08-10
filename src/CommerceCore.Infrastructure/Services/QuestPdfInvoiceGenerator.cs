using CommerceCore.Application.Common.Interfaces;
using CommerceCore.Contracts.Billing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CommerceCore.Infrastructure.Services;

/// <summary>
/// Renders a B2C Indian GST tax invoice as a PDF using QuestPDF.
///
/// LICENSING NOTE: QuestPDF's Community license is free for organizations with
/// under $1M USD annual gross revenue; above that threshold it requires a paid
/// commercial license (https://www.questpdf.com/pricing.html). This is set via
/// QuestPDF.Settings.License in Infrastructure's DependencyInjection — check that
/// your usage qualifies before relying on this in production, or swap in a
/// different PDF library (e.g. PdfSharpCore, which is fully open-source/MIT) by
/// re-implementing IInvoicePdfGenerator if it doesn't.
/// </summary>
public class QuestPdfInvoiceGenerator : IInvoicePdfGenerator
{
    public byte[] Generate(InvoiceDto invoice)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Element(c => ComposeHeader(c, invoice));
                page.Content().Element(c => ComposeContent(c, invoice));
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("This is a computer-generated invoice.").FontSize(8).FontColor(Colors.Grey.Medium);
                });
            });
        });

        return document.GeneratePdf();
    }

    private static void ComposeHeader(QuestPDF.Infrastructure.IContainer container, InvoiceDto invoice)
    {
        container.Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(invoice.SellerLegalName).Bold().FontSize(14);
                    if (!string.IsNullOrWhiteSpace(invoice.SellerGstNumber))
                        col.Item().Text($"GSTIN: {invoice.SellerGstNumber}");
                    if (!string.IsNullOrWhiteSpace(invoice.SellerPanNumber))
                        col.Item().Text($"PAN: {invoice.SellerPanNumber}");
                    col.Item().Text(FormatAddress(invoice.SellerAddressLine1, invoice.SellerAddressLine2, invoice.SellerCity, invoice.SellerState, invoice.SellerPostalCode));
                });

                row.ConstantItem(160).Column(col =>
                {
                    col.Item().AlignRight().Text("TAX INVOICE").Bold().FontSize(16);
                    col.Item().AlignRight().Text($"Invoice No: {invoice.InvoiceNumber}");
                    col.Item().AlignRight().Text($"Date: {invoice.InvoiceDate:dd-MMM-yyyy}");
                    col.Item().AlignRight().Text($"FY: {invoice.FinancialYear}");
                });
            });

            column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
        });
    }

    private static void ComposeContent(QuestPDF.Infrastructure.IContainer container, InvoiceDto invoice)
    {
        container.PaddingTop(15).Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Bill To:").Bold();
                    col.Item().Text(invoice.BuyerName);
                    col.Item().Text(FormatAddress(invoice.BuyerAddressLine1, invoice.BuyerAddressLine2, invoice.BuyerCity, invoice.BuyerState, invoice.BuyerPostalCode));
                    if (!string.IsNullOrWhiteSpace(invoice.BuyerPhoneNumber))
                        col.Item().Text($"Phone: {invoice.BuyerPhoneNumber}");
                });

                row.RelativeItem().AlignRight().Column(col =>
                {
                    col.Item().Text($"Place of Supply: {invoice.PlaceOfSupplyState}");
                    col.Item().Text(invoice.IsInterStateSupply ? "Supply Type: Inter-State (IGST)" : "Supply Type: Intra-State (CGST + SGST)");
                });
            });

            column.Item().PaddingTop(15).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(20);   // #
                    columns.RelativeColumn(3);     // Description
                    columns.ConstantColumn(50);    // HSN
                    columns.ConstantColumn(30);    // Qty
                    columns.ConstantColumn(50);    // Rate
                    columns.ConstantColumn(55);    // Taxable Value
                    columns.ConstantColumn(35);    // GST%
                    columns.ConstantColumn(45);    // CGST
                    columns.ConstantColumn(45);    // SGST
                    columns.ConstantColumn(45);    // IGST
                    columns.ConstantColumn(55);    // Total
                });

                static QuestPDF.Infrastructure.IContainer HeaderStyle(QuestPDF.Infrastructure.IContainer c)
                    => c.Background(Colors.Grey.Lighten3).Padding(3).DefaultTextStyle(t => t.Bold().FontSize(8));

                static QuestPDF.Infrastructure.IContainer BodyStyle(QuestPDF.Infrastructure.IContainer c)
                    => c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).DefaultTextStyle(t => t.FontSize(8));

                table.Header(header =>
                {
                    header.Cell().Element(HeaderStyle).Text("#");
                    header.Cell().Element(HeaderStyle).Text("Description");
                    header.Cell().Element(HeaderStyle).Text("HSN");
                    header.Cell().Element(HeaderStyle).Text("Qty");
                    header.Cell().Element(HeaderStyle).Text("Rate");
                    header.Cell().Element(HeaderStyle).Text("Taxable");
                    header.Cell().Element(HeaderStyle).Text("GST%");
                    header.Cell().Element(HeaderStyle).Text("CGST");
                    header.Cell().Element(HeaderStyle).Text("SGST");
                    header.Cell().Element(HeaderStyle).Text("IGST");
                    header.Cell().Element(HeaderStyle).Text("Total");
                });

                var index = 1;
                foreach (var item in invoice.Items)
                {
                    var description = item.VariantDisplayName != null ? $"{item.ProductName} ({item.VariantDisplayName})" : item.ProductName;

                    table.Cell().Element(BodyStyle).Text((index++).ToString());
                    table.Cell().Element(BodyStyle).Text($"{description}\nSKU: {item.Sku}");
                    table.Cell().Element(BodyStyle).Text(item.HsnCode ?? "-");
                    table.Cell().Element(BodyStyle).Text(item.Quantity.ToString());
                    table.Cell().Element(BodyStyle).Text(item.UnitPrice.ToString("N2"));
                    table.Cell().Element(BodyStyle).Text(item.TaxableValue.ToString("N2"));
                    table.Cell().Element(BodyStyle).Text(item.GstRatePercentage.ToString("N1"));
                    table.Cell().Element(BodyStyle).Text(item.CgstAmount.ToString("N2"));
                    table.Cell().Element(BodyStyle).Text(item.SgstAmount.ToString("N2"));
                    table.Cell().Element(BodyStyle).Text(item.IgstAmount.ToString("N2"));
                    table.Cell().Element(BodyStyle).Text(item.LineTotal.ToString("N2"));
                }
            });

            column.Item().PaddingTop(15).AlignRight().Column(col =>
            {
                col.Item().Width(220).Row(r => { r.RelativeItem().Text("Taxable Value"); r.ConstantItem(80).AlignRight().Text(invoice.TaxableValue.ToString("N2")); });
                if (invoice.TotalDiscountAmount > 0)
                    col.Item().Width(220).Row(r => { r.RelativeItem().Text("Discount"); r.ConstantItem(80).AlignRight().Text($"-{invoice.TotalDiscountAmount:N2}"); });
                if (invoice.TotalCgstAmount > 0)
                    col.Item().Width(220).Row(r => { r.RelativeItem().Text("Total CGST"); r.ConstantItem(80).AlignRight().Text(invoice.TotalCgstAmount.ToString("N2")); });
                if (invoice.TotalSgstAmount > 0)
                    col.Item().Width(220).Row(r => { r.RelativeItem().Text("Total SGST"); r.ConstantItem(80).AlignRight().Text(invoice.TotalSgstAmount.ToString("N2")); });
                if (invoice.TotalIgstAmount > 0)
                    col.Item().Width(220).Row(r => { r.RelativeItem().Text("Total IGST"); r.ConstantItem(80).AlignRight().Text(invoice.TotalIgstAmount.ToString("N2")); });

                col.Item().Width(220).PaddingTop(5).BorderTop(1).BorderColor(Colors.Grey.Lighten1)
                    .Row(r => { r.RelativeItem().Text("Grand Total").Bold(); r.ConstantItem(80).AlignRight().Text($"₹ {invoice.TotalAmount:N2}").Bold(); });
            });
        });
    }

    private static string FormatAddress(string? line1, string? line2, string? city, string? state, string? postalCode)
    {
        var parts = new[] { line1, line2, city, state, postalCode }.Where(p => !string.IsNullOrWhiteSpace(p));
        return string.Join(", ", parts);
    }
}
