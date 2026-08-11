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
    private static decimal RoundMoney(decimal value)
        => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    public static GstBreakdown Calculate(decimal taxableValue, decimal gstRatePercentage, bool isInterState)
    {
        if (taxableValue <= 0 || gstRatePercentage <= 0)
            return new GstBreakdown(0, 0, 0);

        if (isInterState)
        {
            var igst = RoundMoney(taxableValue * gstRatePercentage / 100m);
            return new GstBreakdown(0, 0, igst);
        }

        var halfRate = gstRatePercentage / 2m;
        var cgst = RoundMoney(taxableValue * halfRate / 100m);
        var sgst = RoundMoney(taxableValue * halfRate / 100m);
        return new GstBreakdown(cgst, sgst, 0);
    }

    public static decimal GetTaxableValueFromGross(decimal grossAmount, decimal gstRatePercentage)
    {
        if (grossAmount <= 0 || gstRatePercentage <= 0)
            return RoundMoney(grossAmount);

        var divisor = 1 + gstRatePercentage / 100m;
        return RoundMoney(grossAmount / divisor);
    }

    public static GstBreakdown CalculateFromGross(decimal grossAmount, decimal gstRatePercentage, bool isInterState)
    {
        if (grossAmount <= 0 || gstRatePercentage <= 0)
            return new GstBreakdown(0, 0, 0);

        var taxableAmount = GetTaxableValueFromGross(grossAmount, gstRatePercentage);
        var totalTax = RoundMoney(grossAmount - taxableAmount);

        if (isInterState)
        {
            return new GstBreakdown(0, 0, totalTax);
        }

        var cgst = RoundMoney(totalTax / 2m);
        var sgst = totalTax - cgst;
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
