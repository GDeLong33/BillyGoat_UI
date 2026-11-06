using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BillyGoat_UI.Models;

/// <summary>
/// A customer account: everything shown on the Account screen.
/// The regions below follow the account sub-screens in order.
/// For the demo, accounts come from Services/FakeData.cs.
/// </summary>
public partial class Account : ObservableObject
{
    public Account()
    {
        Status = "Active";
    }

    public required string AccountNumber { get; init; }   // e.g. "A-01842"

    // ===== 1. Contact information =====
    public Address Billing { get; } = new();
    public Address Service { get; } = new();

    /// <summary>A second name for the service address (some properties have two owners).</summary>
    public string SecondServiceName { get; set; } = "";

    public string HomePhone { get; set; } = "";
    public string WorkPhone { get; set; } = "";
    public string WorkExtension { get; set; } = "";
    public string MobilePhone { get; set; } = "";
    public string Email { get; set; } = "";
    public string ContactTitle { get; set; } = "Mr";      // Mr / Mrs / Ms / Other

    // ===== 2. Details =====
    public DateTime StartDate { get; set; }

    /// <summary>Active, Inactive, Cancelled, On hold or Prospect (see Lookups.AccountStatuses).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsActive))]
    public partial string Status { get; set; }

    [ObservableProperty]
    public partial DateTime StatusDate { get; set; }

    public string StatusDescription { get; set; } = "";
    public string PropertyType { get; set; } = "";
    public string CrossStreet { get; set; } = "";
    public string Directions { get; set; } = "";
    public bool SignatureOnFile { get; set; }
    public bool Prenotification { get; set; }
    public bool DontAutoroute { get; set; }
    public bool SeniorCitizen { get; set; }

    // Service defaults: copied into each new service sold to this account.
    public int Measurement { get; set; }                  // lawn size in square feet
    public string Program { get; set; } = "";
    public string Route { get; set; } = "";
    public string Frequency { get; set; } = "";
    public string Sort { get; set; } = "";
    public string Source { get; set; } = "";
    public string Salesperson { get; set; } = "";
    public string PrenotifyType { get; set; } = "";

    // Record information (read-only on screen).
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = "";
    public DateTime EditedDate { get; set; }
    public string EditedBy { get; set; } = "";

    // ===== 3. Pages and notes =====
    public ObservableCollection<OfficeNote> Notes { get; } = new();
    public string SpecialInstructions { get; set; } = "";

    // ===== 4. Services =====
    public ObservableCollection<ServiceRecord> Services { get; } = new();

    // ===== 5. Accounting defaults =====
    public DateTime? LastStatementSent { get; set; }
    public bool DontSendStatements { get; set; }
    public bool DontChargeInterest { get; set; }
    public bool DontFinancialHold { get; set; }
    public bool ReconfirmYearly { get; set; }
    public bool CancelLetterSent { get; set; }
    public bool WelcomeLetterSent { get; set; }
    public bool HoldCurrentPrices { get; set; }
    public DateTime? ConfirmationDate { get; set; }
    public string ConfirmedBy { get; set; } = "";
    public string DiscountType { get; set; } = "None";
    public string CardType { get; set; } = "None";
    public string BillingType { get; set; } = "Normal";   // Normal / Main / Sub / Corporate
    public string PayType { get; set; } = "Normal";       // Normal / Pay plan / Monthly - show price / Monthly - no price

    // ===== 6. Accounting adjustments =====
    public ObservableCollection<AdjustmentEntry> Adjustments { get; } = new();

    // ===== 7. Marketing letters =====
    [ObservableProperty]
    public partial decimal PrepayPercent { get; set; }

    [ObservableProperty]
    public partial string RenewalLetter { get; set; } = "";

    public ObservableCollection<LetterRecord> Letters { get; } = new();

    // ===== Read-only text used by lists and headers =====

    public bool IsActive => Status == "Active";

    /// <summary>"Whitfield, Rachel"</summary>
    public string BillingName => Billing.DisplayName;
    public string BillingAddressText => Billing.OneLine;
    public string ServiceAddressText => Service.OneLine;

    /// <summary>"Park, Jin" or "Park, Jin; Park, Min-ji" when the property has two names.</summary>
    public string ServiceNameText =>
        SecondServiceName.Length > 0 ? $"{Service.DisplayName}; {SecondServiceName}" : Service.DisplayName;

    /// <summary>"8,400 sq ft"</summary>
    public string MeasurementText => Measurement > 0 ? $"{Measurement.ToString("N0", Formats.Us)} sq ft" : "";

    /// <summary>Total still owed on all services.</summary>
    public decimal Balance => Services.Where(s => s.Status != ServiceRecord.Inactive).Sum(s => s.Balance);

    /// <summary>
    /// Call after editing the plain (non-observable) properties, so that lists and
    /// headers showing this account refresh their text.
    /// </summary>
    public void RefreshDisplay()
    {
        OnPropertyChanged(nameof(BillingName));
        OnPropertyChanged(nameof(BillingAddressText));
        OnPropertyChanged(nameof(ServiceAddressText));
        OnPropertyChanged(nameof(ServiceNameText));
        OnPropertyChanged(nameof(MeasurementText));
        OnPropertyChanged(nameof(Balance));
    }
}

/// <summary>Which field the Search tab looks in.</summary>
public enum AccountSearchField
{
    LastName,
    FirstName,
    AccountNumber,
    StreetAddress
}

/// <summary>Active / All / Inactive filter under the search results.</summary>
public enum AccountStatusFilter
{
    Active,
    All,
    Inactive
}
