using System.Globalization;

namespace BillyGoat_UI.Models;

/// <summary>
/// Number and date formats used everywhere. We always use US formats, so money
/// shows as "$1,860.00" and dates as "10/06/2026" no matter the PC's region settings.
/// </summary>
public static class Formats
{
    public static readonly CultureInfo Us = CultureInfo.GetCultureInfo("en-US");

    /// <summary>"$1,860.00"</summary>
    public static string Money(decimal amount) => amount.ToString("C2", Us);

    /// <summary>"10/06/2026", or an empty string for no date.</summary>
    public static string Date(DateTime? date) => date?.ToString("MM/dd/yyyy", Us) ?? "";
}
