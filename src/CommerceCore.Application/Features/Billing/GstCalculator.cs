namespace CommerceCore.Application.Features.Billing;

public readonly record struct GstBreakdown(decimal Cgst, decimal Sgst, decimal Igst)
{
    public decimal Total => Cgst + Sgst + Igst;
}

/// <summary>
/// Indian GST split logic: a sale is either intra-state (seller and buyer in the
/// same state) — split evenly into CGST + SGST, each half the product's GST rate —
/// or inter-state — charged entirely as IGST at the full rate. Exactly one of the
/// two applies per line, never both, per GST law.
/// </summary>
public static class GstCalculator
{
    public static GstBreakdown Calculate(decimal taxableValue, decimal gstRatePercentage, bool isInterState)
    {
        if (taxableValue <= 0 || gstRatePercentage <= 0)
            return new GstBreakdown(0, 0, 0);

        if (isInterState)
        {
            var igst = Math.Round(taxableValue * gstRatePercentage / 100m, 2, MidpointRounding.AwayFromZero);
            return new GstBreakdown(0, 0, igst);
        }

        var halfRate = gstRatePercentage / 2m;
        var cgst = Math.Round(taxableValue * halfRate / 100m, 2, MidpointRounding.AwayFromZero);
        var sgst = Math.Round(taxableValue * halfRate / 100m, 2, MidpointRounding.AwayFromZero);
        return new GstBreakdown(cgst, sgst, 0);
    }

    /// <summary>Compares two state names the way Indian addresses are actually
    /// entered — case/whitespace-insensitive — since Address.State and
    /// StoreSettings' registered state are both free-text, not FK-linked to a
    /// canonical States table row. See the Address entity's doc comment.</summary>
    public static bool IsSameState(string? sellerState, string? buyerState)
    {
        if (string.IsNullOrWhiteSpace(sellerState) || string.IsNullOrWhiteSpace(buyerState))
            return true; // can't determine — default to intra-state (CGST+SGST) rather than guess IGST

        return string.Equals(sellerState.Trim(), buyerState.Trim(), StringComparison.OrdinalIgnoreCase);
    }
}
