using System.Collections.ObjectModel;
using BillyGoat_UI.Models;
using BillyGoat_UI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BillyGoat_UI.ViewModels;

/// <summary>A material used on a job (Material entry section).</summary>
public class MaterialLine
{
    public required string Material { get; init; }
    public required string Quantity { get; init; }
    public required string Unit { get; init; }
}

/// <summary>A line in "Entered in this batch" after Post.</summary>
public class PostedLine
{
    public required string InvoiceNumber { get; init; }
    public required string AccountText { get; init; }
    public required string ServiceName { get; init; }
    public required string Employee { get; init; }
    public required decimal Amount { get; init; }
}

/// <summary>
/// Office tool 1: Enter Completed Services. The crew hands in their work orders;
/// the office types (or scans) each invoice number and posts it as completed.
/// </summary>
public partial class CompletedServicesViewModel : ObservableObject
{
    private readonly Action _close;

    public CompletedServicesViewModel(Batch batch, Action close)
    {
        Batch = batch;
        _close = close;
        DateCompleted = batch.Date;
        InvoiceNumber = "";
        AdjustmentDescription = "";
        AdjustmentAmountText = "";
        TimeIn = "";
        TimeOut = "";
        Material = Materials.First();
        Quantity = "";
        Unit = Units.First();
        StatusMessage = "";
        ErrorMessage = "";

        // If an applicator is signed in, pick their name as the employee.
        string me = Session.UserName;
        Employee = Employees.FirstOrDefault(e => me.Length > 0 && me.Contains(e.Split(", ")[0]));
    }

    public Batch Batch { get; }
    public List<string> Employees { get; } = FakeData.ActiveEmployeeNames;
    public string[] Materials { get; } = ["Fertilizer 24-0-6", "Fertilizer 28-0-3", "Grub control (granular)", "Broadleaf weed control", "Hardwood mulch", "Grass seed"];
    public string[] Units { get; } = ["lbs", "gal", "bags", "cu yd"];

    /// <summary>A few open invoice numbers, so the demo user knows what to type.</summary>
    public string SampleInvoicesText => "Try one of these open invoices: " + string.Join(", ",
        FakeData.Accounts.Where(a => a.IsActive)
            .SelectMany(a => a.Services.Where(s => s.IsOutstanding).Select(s => s.InvoiceNumber))
            .Take(5));

    // ===== Service information =====

    [ObservableProperty] public partial DateTime DateCompleted { get; set; }
    [ObservableProperty] public partial string? Employee { get; set; }

    /// <summary>Barcode / invoice number. Looking it up happens as you type.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsInvoiceFound))]
    [NotifyPropertyChangedFor(nameof(InvoiceInfoText))]
    [NotifyPropertyChangedFor(nameof(InvoiceAmountText))]
    public partial string InvoiceNumber { get; set; }

    private Account? _account;
    private ServiceRecord? _service;

    partial void OnInvoiceNumberChanged(string value)
    {
        var found = FakeData.FindInvoice(value);
        _account = found?.Account;
        _service = found?.Service;
        ErrorMessage = "";
    }

    /// <summary>True once a known invoice number is typed. Shows the invoice section.</summary>
    public bool IsInvoiceFound => _service != null;

    public string InvoiceInfoText => _service == null
        ? (InvoiceNumber.Trim().Length >= 5 ? "No service has this invoice number." : "")
        : $"{_account!.AccountNumber}  {_account.ServiceNameText}  -  {_service.ServiceName}  ({_service.Status}, scheduled {Formats.Date(_service.DateScheduled)})";

    public string InvoiceAmountText => _service == null ? "" : Formats.Money(_service.SaleAmount);

    [ObservableProperty] public partial string AdjustmentDescription { get; set; }
    [ObservableProperty] public partial string AdjustmentAmountText { get; set; }

    // ===== Time entry =====
    [ObservableProperty] public partial string TimeIn { get; set; }
    [ObservableProperty] public partial string TimeOut { get; set; }

    // ===== Material entry =====
    [ObservableProperty] public partial string Material { get; set; }
    [ObservableProperty] public partial string Quantity { get; set; }
    [ObservableProperty] public partial string Unit { get; set; }
    public ObservableCollection<MaterialLine> MaterialLines { get; } = new();

    [RelayCommand]
    private void AddMaterial()
    {
        if (Quantity.Trim().Length == 0)
        {
            ErrorMessage = "Type the quantity of material used.";
            return;
        }
        MaterialLines.Add(new MaterialLine { Material = Material, Quantity = Quantity.Trim(), Unit = Unit });
        Quantity = "";
        ErrorMessage = "";
    }

    // ===== Results =====
    public ObservableCollection<PostedLine> PostedLines { get; } = new();
    [ObservableProperty] public partial string StatusMessage { get; set; }
    [ObservableProperty] public partial string ErrorMessage { get; set; }

    // ===== Buttons =====

    [RelayCommand]
    private void AddEstimate() =>
        MessageService.Show($"An estimate would be created for {_account?.BillingName}.\n\n(Estimates are not part of this demo.)");

    /// <summary>Add service: the crew sold extra work on site. Opens the normal Add Service dialog.</summary>
    [RelayCommand]
    private void AddService()
    {
        if (_account != null)
            DialogService.ShowDialog(new ServiceEditorViewModel(_account, existing: null));
    }

    /// <summary>Add request: the customer asked for something. Saved as an office note on the account.</summary>
    [RelayCommand]
    private void AddRequest()
    {
        if (_account == null) return;
        _account.Notes.Insert(0, new OfficeNote
        {
            Timestamp = FakeData.Now, EnteredBy = Session.UserName,
            Text = $"Request from the crew on invoice {_service!.InvoiceNumber}: customer asked to be called about extra work.",
        });
        MessageService.Show($"A request was added to the office notes of {_account.BillingName}.");
    }

    /// <summary>Post: marks the service as completed and adds it to "Entered in this batch".</summary>
    [RelayCommand]
    private void Post()
    {
        ErrorMessage = "";
        if (_service == null || _account == null) { ErrorMessage = "Type a valid invoice number first."; return; }
        if (string.IsNullOrEmpty(Employee)) { ErrorMessage = "Choose the employee who did the work."; return; }
        if (_service.Status == ServiceRecord.Completed) { ErrorMessage = $"Invoice {_service.InvoiceNumber} is already completed."; return; }

        decimal adjustment = 0;
        if (AdjustmentAmountText.Trim().Length > 0 &&
            !decimal.TryParse(AdjustmentAmountText, System.Globalization.NumberStyles.Currency, Formats.Us, out adjustment))
        {
            ErrorMessage = "Price adjustment amount must be a number, for example -5.00.";
            return;
        }

        // Update the service and the account's accounting history.
        _service.Status = ServiceRecord.Completed;
        _service.DateCompleted = DateCompleted;
        _service.AssignedEmployee = Employee;
        _service.Adjustments += adjustment;
        _account.Adjustments.Add(new AdjustmentEntry
        {
            EntryDate = DateCompleted, Transaction = "Sale", Type = "Invoice", Batch = Batch.Number, Service = _service,
            Memo = $"{_service.ServiceName} (invoice {_service.InvoiceNumber})", Total = _service.SaleAmount,
        });
        if (adjustment != 0)
        {
            _account.Adjustments.Add(new AdjustmentEntry
            {
                EntryDate = DateCompleted, Transaction = "Price adjustment", Type = "Adjustment", Batch = Batch.Number, Service = _service,
                Memo = AdjustmentDescription.Trim().Length > 0 ? AdjustmentDescription.Trim() : "Price adjustment", Total = adjustment,
            });
        }

        PostedLines.Add(new PostedLine
        {
            InvoiceNumber = _service.InvoiceNumber, AccountText = $"{_account.AccountNumber} {_account.BillingName}",
            ServiceName = _service.ServiceName, Employee = Employee, Amount = _service.SaleAmount + adjustment,
        });
        StatusMessage = $"Posted invoice {_service.InvoiceNumber} to batch {Batch.Number}.";

        // Clear the invoice fields for the next work order (keep date and employee).
        InvoiceNumber = "";
        AdjustmentDescription = "";
        AdjustmentAmountText = "";
        MaterialLines.Clear();
        OnPropertyChanged(nameof(SampleInvoicesText));
    }

    [RelayCommand]
    private void Close() => _close();
}
