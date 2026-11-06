namespace BillyGoat_UI.Models;

// Small record types that belong to an account. They are kept together in one
// file because each is only a few lines long.

/// <summary>A time-stamped office note on the "Pages and Notes" screen.</summary>
public class OfficeNote
{
    public required DateTime Timestamp { get; init; }
    public required string Text { get; init; }
    public required string EnteredBy { get; init; }

    /// <summary>"Tue 10/06/2026  9:14 AM"</summary>
    public string StampText => Timestamp.ToString("ddd MM/dd/yyyy  h:mm tt", Formats.Us);
}

/// <summary>
/// One line in the account's accounting history (a sale, a payment, a credit...).
/// Shown on the "Accounting Adjustments" screen.
/// </summary>
public class AdjustmentEntry
{
    public required DateTime EntryDate { get; init; }
    public required string Transaction { get; init; }   // Sale, Payment, Credit, Finance charge...
    public required string Type { get; init; }          // Check, Cash, Credit card, Invoice...
    public string Memo { get; init; } = "";
    public required decimal Total { get; init; }
    public string Batch { get; init; } = "";
    public bool Journalized { get; init; }
    public ServiceRecord? Service { get; init; }         // the service this entry belongs to, if any

    public string JournalizedText => Journalized ? "Yes" : "No";
}

/// <summary>A record of a marketing letter (prepay or renewal) sent to the account.</summary>
public class LetterRecord
{
    public required DateTime DateSent { get; init; }
    public required string LetterType { get; init; }    // Upsell, Non upsell, Renewal...
    public required string Services { get; init; }
    public decimal PrepayPercent { get; init; }
    public decimal PrepayAmount { get; init; }
    public DateTime? ExpirationDate { get; init; }
}

/// <summary>
/// A batch groups entries typed in one sitting (completed services or payments),
/// so they can be checked and posted together, like in Service Pro 2000.
/// </summary>
public class Batch
{
    public required string Number { get; init; }        // e.g. "P-1041"
    public required DateTime Date { get; init; }
    public required string Kind { get; init; }          // "Production" or "Payment"
    public bool IsPosted { get; set; }

    public string DisplayName => $"{Number}  ({Date.ToString("MM/dd/yyyy", Formats.Us)})";
}
