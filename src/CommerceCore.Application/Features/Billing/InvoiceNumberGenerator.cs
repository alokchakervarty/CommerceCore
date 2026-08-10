using CommerceCore.Application.Common.Interfaces;
using CommerceCore.Domain.Entities.Billing;
using Microsoft.EntityFrameworkCore;

namespace CommerceCore.Application.Features.Billing;

/// <summary>
/// Generates sequential invoice numbers per Store per Indian financial year
/// (April 1 – March 31), in the form "INV/2526/000001" for FY 2025-26. Backed by
/// InvoiceSequence, incremented atomically as part of the same SaveChangesAsync
/// call that creates the Order — see CheckoutCommandHandler, which retries once on
/// a concurrency conflict (two simultaneous checkouts racing for the same store's
/// next number) by reloading the sequence row and trying again.
/// </summary>
public static class InvoiceNumberGenerator
{
    public static string GetIndianFinancialYear(DateTime date)
    {
        var startYear = date.Month >= 4 ? date.Year : date.Year - 1;
        var endYear = startYear + 1;
        return $"{startYear}-{endYear % 100:D2}";
    }

    public static async Task<string> GetNextNumberAsync(
        IApplicationDbContext db, Guid storeId, DateTime invoiceDate, CancellationToken cancellationToken)
    {
        var financialYear = GetIndianFinancialYear(invoiceDate);

        var sequence = await db.InvoiceSequences.FirstOrDefaultAsync(
            s => s.StoreId == storeId && s.FinancialYear == financialYear, cancellationToken);

        if (sequence == null)
        {
            sequence = new InvoiceSequence { StoreId = storeId, FinancialYear = financialYear, LastNumber = 0 };
            db.InvoiceSequences.Add(sequence);
        }

        sequence.LastNumber += 1;

        var parts = financialYear.Split('-');
        var fyCode = $"{parts[0][2..]}{parts[1]}"; // "2025-26" -> "2526"

        return $"INV/{fyCode}/{sequence.LastNumber:D6}";
    }
}
