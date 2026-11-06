namespace BillyGoat_UI.Services;

/// <summary>
/// Standard prices for the demo. Most lawn prices depend on the lawn size,
/// so price = base price + an amount per 1,000 square feet.
/// The office can still change the price for one customer (Edit Service).
/// </summary>
public static class PriceList
{
    // (base price, price per 1,000 sq ft)
    private static readonly Dictionary<string, (decimal Base, decimal Per1000)> Prices = new()
    {
        ["Mowing"] = (25m, 3m),
        ["Fertilization"] = (30m, 2.5m),
        ["Grub control"] = (40m, 3.5m),
        ["Mulch work"] = (150m, 15m),
        ["Bed maintenance"] = (60m, 5m),
        ["Shrub trimming"] = (90m, 6m),
        ["Spring cleanup"] = (110m, 6m),
        ["Fall cleanup"] = (120m, 6m),
        ["Aeration"] = (60m, 4m),
    };

    /// <summary>The standard price for a service on a lawn of the given size, rounded to whole dollars.</summary>
    public static decimal StandardPrice(string serviceName, int squareFeet)
    {
        // "Fertilization round 2" uses the "Fertilization" price.
        string? key = Prices.Keys.FirstOrDefault(k => serviceName.StartsWith(k, StringComparison.OrdinalIgnoreCase));
        (decimal basePrice, decimal per1000) = key != null ? Prices[key] : (50m, 3m);
        return Math.Round(basePrice + per1000 * squareFeet / 1000m);
    }
}
