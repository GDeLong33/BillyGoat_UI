using CommunityToolkit.Mvvm.ComponentModel;

namespace BillyGoat_UI.Models;

/// <summary>
/// One service sold to an account, for example "Fertilization round 3" on June 2.
/// Each service has its own invoice number, which is printed as a barcode on the work order.
///
/// It is an ObservableObject because some values change while screens are open
/// (printing, completing and paying a service), and the tables must update.
/// </summary>
public partial class ServiceRecord : ObservableObject
{
    // Status values used throughout the app.
    public const string Open = "Open";
    public const string Printed = "Printed";
    public const string Completed = "Completed";
    public const string Inactive = "Inactive";

    public ServiceRecord()
    {
        Status = Open;
    }

    // ----- Fixed information about the sale -----
    public int ServiceId { get; set; }
    public string InvoiceNumber { get; set; } = "";
    public string ServiceName { get; set; } = "";
    public string ProgramName { get; set; } = "";     // filled in when the service is part of a program
    public DateTime DateSold { get; set; }
    public string OkayedBy { get; set; } = "";
    public DateTime DateScheduled { get; set; }
    public string AssignedEmployee { get; set; } = "";
    public string Route { get; set; } = "";
    public string Frequency { get; set; } = "";
    public string Sort { get; set; } = "";
    public string Salesperson { get; set; } = "";
    public string PrenotifyType { get; set; } = "";
    public decimal Units { get; set; } = 1;
    public bool Renewable { get; set; } = true;
    public bool ServiceThisYear { get; set; } = true;
    public bool ConfirmationSent { get; set; }
    public string Instructions { get; set; } = "";

    // ----- Values that change -----

    /// <summary>Open, Printed, Completed or Inactive.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusCode))]
    public partial string Status { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Balance))]
    public partial decimal SaleAmount { get; set; }

    [ObservableProperty]
    public partial DateTime? DatePrinted { get; set; }

    [ObservableProperty]
    public partial DateTime? DateCompleted { get; set; }

    /// <summary>Total money received for this service.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Balance))]
    public partial decimal Payments { get; set; }

    /// <summary>Price adjustments: negative = credit or discount, positive = extra charge.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Balance))]
    public partial decimal Adjustments { get; set; }

    /// <summary>What the customer still owes: sale + adjustments - payments.</summary>
    public decimal Balance => SaleAmount + Adjustments - Payments;

    /// <summary>One-letter status for narrow columns: O, P, C or I.</summary>
    public string StatusCode => Status.Length > 0 ? Status[..1] : "";

    /// <summary>True when the work is still to be done (not completed and not inactive).</summary>
    public bool IsOutstanding => Status == Open || Status == Printed;
}
