using BillyGoat_UI.Models;
using BillyGoat_UI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BillyGoat_UI.ViewModels;

/// <summary>
/// The Add Service and Edit Service dialog (one dialog, two modes).
///
///   Add:  a new invoice number is generated, the fields start from the account's
///         service defaults, and the price comes from the standard price list.
///   Edit: the fields show the existing service, and the price for this customer
///         can be changed.
///
/// The dialog works on copies of the values, so Cancel throws the changes away.
/// OK saves and closes; Apply saves and keeps the dialog open.
/// </summary>
public partial class ServiceEditorViewModel : DialogViewModel
{
    private readonly Account _account;
    private ServiceRecord? _service;   // null until a new service is saved

    public ServiceEditorViewModel(Account account, ServiceRecord? existing)
    {
        _account = account;
        _service = existing;
        StatusMessage = "";
        ErrorMessage = "";

        if (existing == null)
        {
            // ----- Add mode: start from the account's defaults -----
            InvoiceNumber = FakeData.NewInvoiceNumber();
            ServiceName = Lookups.Services.FirstOrDefault() ?? "";
            DateSold = FakeData.Today;
            OkayedBy = account.Billing.FirstName;
            DateScheduled = FakeData.Today.AddDays(7);
            AssignedEmployee = "";
            Route = account.Route;
            Frequency = account.Frequency;
            Sort = account.Sort;
            Salesperson = account.Salesperson;
            PrenotifyType = account.PrenotifyType;
            Units = "1";
            Renewable = true;
            ServiceThisYear = true;
            Instructions = "";
            AmountText = PriceList.StandardPrice(ServiceName, account.Measurement).ToString("0.00");
        }
        else
        {
            // ----- Edit mode: copy the service's values -----
            InvoiceNumber = existing.InvoiceNumber;
            ServiceName = existing.ServiceName;
            DateSold = existing.DateSold;
            OkayedBy = existing.OkayedBy;
            DateScheduled = existing.DateScheduled;
            AssignedEmployee = existing.AssignedEmployee;
            Route = existing.Route;
            Frequency = existing.Frequency;
            Sort = existing.Sort;
            Salesperson = existing.Salesperson;
            PrenotifyType = existing.PrenotifyType;
            Units = existing.Units.ToString("0.##");
            Renewable = existing.Renewable;
            ServiceThisYear = existing.ServiceThisYear;
            ConfirmationSent = existing.ConfirmationSent;
            Instructions = existing.Instructions;
            AmountText = existing.SaleAmount.ToString("0.00");
        }
    }

    public override string Title => IsEditMode ? "Edit Service" : "Add Service";

    /// <summary>True when editing a saved service. In this mode the price can be changed.</summary>
    public bool IsEditMode => _service != null;
    public bool IsAddMode => !IsEditMode;

    public string AccountText => $"{_account.AccountNumber}  {_account.BillingName}";

    /// <summary>Auto-generated, shown read-only.</summary>
    public string InvoiceNumber { get; }

    // Drop-down lists (shared with the rest of the app, see Services/Lookups.cs).
    public List<string> Employees { get; } = FakeData.ActiveEmployeeNames;

    [ObservableProperty]
    public partial string ServiceName { get; set; }

    [ObservableProperty] public partial DateTime DateSold { get; set; }
    [ObservableProperty] public partial string OkayedBy { get; set; }
    [ObservableProperty] public partial DateTime DateScheduled { get; set; }
    [ObservableProperty] public partial string AssignedEmployee { get; set; }
    [ObservableProperty] public partial string Route { get; set; }
    [ObservableProperty] public partial string Frequency { get; set; }
    [ObservableProperty] public partial string Sort { get; set; }
    [ObservableProperty] public partial string Salesperson { get; set; }
    [ObservableProperty] public partial string PrenotifyType { get; set; }
    [ObservableProperty] public partial string Units { get; set; }
    [ObservableProperty] public partial bool Renewable { get; set; }
    [ObservableProperty] public partial bool ServiceThisYear { get; set; }
    [ObservableProperty] public partial bool ConfirmationSent { get; set; }
    [ObservableProperty] public partial string Instructions { get; set; }

    /// <summary>Amount of sale, as typed (checked when saving).</summary>
    [ObservableProperty] public partial string AmountText { get; set; }

    /// <summary>Under the amount: explains where the price comes from.</summary>
    public string PriceNote => IsEditMode
        ? "You can change the price for this customer."
        : $"Standard price for {_account.MeasurementText}. After saving, use Edit Service to change the price for this customer.";

    /// <summary>Green "Saved" message after Apply.</summary>
    [ObservableProperty] public partial string StatusMessage { get; set; }
    [ObservableProperty] public partial string ErrorMessage { get; set; }

    // In Add mode, picking a different service fills in its standard price.
    partial void OnServiceNameChanged(string value)
    {
        if (IsAddMode && value != null)
            AmountText = PriceList.StandardPrice(value, _account.Measurement).ToString("0.00");
    }

    [RelayCommand]
    private void Ok()
    {
        if (Save())
            Close(true);
    }

    [RelayCommand]
    private void Apply()
    {
        if (Save())
            StatusMessage = $"Saved at {DateTime.Now:h:mm tt}.";
    }

    [RelayCommand]
    private void Cancel() => Close(false);

    /// <summary>Checks the input, then copies the values into the service. Returns false if something is wrong.</summary>
    private bool Save()
    {
        ErrorMessage = "";
        if (string.IsNullOrWhiteSpace(ServiceName))
        {
            ErrorMessage = "Choose a service.";
            return false;
        }
        if (!decimal.TryParse(AmountText, System.Globalization.NumberStyles.Currency, Formats.Us, out decimal amount) || amount < 0)
        {
            ErrorMessage = "Amount of sale must be a number, for example 52.00.";
            return false;
        }
        if (!decimal.TryParse(Units, out decimal units) || units <= 0)
        {
            ErrorMessage = "Units must be a number greater than 0.";
            return false;
        }

        bool isNew = _service == null;
        _service ??= new ServiceRecord { ServiceId = FakeData.NewServiceId(), InvoiceNumber = InvoiceNumber };

        _service.ServiceName = ServiceName;
        _service.DateSold = DateSold;
        _service.OkayedBy = OkayedBy;
        _service.DateScheduled = DateScheduled;
        _service.AssignedEmployee = AssignedEmployee ?? "";
        _service.Route = Route ?? "";
        _service.Frequency = Frequency ?? "";
        _service.Sort = Sort ?? "";
        _service.Salesperson = Salesperson ?? "";
        _service.PrenotifyType = PrenotifyType ?? "";
        _service.Units = units;
        _service.Renewable = Renewable;
        _service.ServiceThisYear = ServiceThisYear;
        _service.ConfirmationSent = ConfirmationSent;
        _service.Instructions = Instructions;
        _service.SaleAmount = amount;

        if (isNew)
        {
            _account.Services.Add(_service);
            // The dialog now edits the saved service, so a second Apply does not add it twice.
            OnPropertyChanged(nameof(IsEditMode));
            OnPropertyChanged(nameof(IsAddMode));
            OnPropertyChanged(nameof(PriceNote));
        }
        _account.EditedDate = FakeData.Today;
        _account.EditedBy = Session.UserName;
        return true;
    }
}
