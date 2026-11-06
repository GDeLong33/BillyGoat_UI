using BillyGoat_UI.Models;
using BillyGoat_UI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BillyGoat_UI.ViewModels;

/// <summary>
/// Add Program dialog. A program is several services sold together
/// (for example the 5-Step Lawn Program = 4 fertilizations + grub control).
/// OK / Apply create one service per step, spread over the season.
/// </summary>
public partial class AddProgramViewModel : DialogViewModel
{
    private readonly Account _account;

    public AddProgramViewModel(Account account)
    {
        _account = account;
        InvoiceNumber = FakeData.NewInvoiceNumber();
        StatusMessage = "";
        ErrorMessage = "";

        // Sales information
        Program = Lookups.Programs.FirstOrDefault() ?? "";
        DateSold = FakeData.Today;
        OkayedBy = account.Billing.FirstName;
        Source = account.Source;
        Status = ServiceRecord.Open;
        MeasurementText = account.Measurement.ToString();
        Salesperson = account.Salesperson;

        // Routing information
        Route = account.Route;
        Frequency = "Every 6 weeks";
        Sort = account.Sort;

        UpdateSteps();
    }

    public override string Title => "Add Program";
    public string AccountText => $"{_account.AccountNumber}  {_account.BillingName}";

    /// <summary>Invoice number of the first service; each later step gets the next number.</summary>
    [ObservableProperty] public partial string InvoiceNumber { get; set; }

    public string[] Statuses { get; } = [ServiceRecord.Open, ServiceRecord.Inactive];

    // ----- Sales information -----
    [ObservableProperty] public partial string Program { get; set; }
    [ObservableProperty] public partial DateTime DateSold { get; set; }
    [ObservableProperty] public partial string OkayedBy { get; set; }
    [ObservableProperty] public partial string Source { get; set; }
    [ObservableProperty] public partial string AmountText { get; set; } = "";
    [ObservableProperty] public partial string Status { get; set; }
    [ObservableProperty] public partial string MeasurementText { get; set; }
    [ObservableProperty] public partial string Salesperson { get; set; }

    // ----- Routing information -----
    [ObservableProperty] public partial string Route { get; set; }
    [ObservableProperty] public partial string Frequency { get; set; }
    [ObservableProperty] public partial string Sort { get; set; }

    /// <summary>Preview of the services the program will create, e.g. "Grub control - 07/01/2027".</summary>
    [ObservableProperty] public partial List<string> Steps { get; set; } = [];

    [ObservableProperty] public partial string StatusMessage { get; set; }
    [ObservableProperty] public partial string ErrorMessage { get; set; }

    partial void OnProgramChanged(string value) => UpdateSteps();
    partial void OnMeasurementTextChanged(string value) => UpdateSteps();
    partial void OnDateSoldChanged(DateTime value) => UpdateSteps();

    /// <summary>The service names in the chosen program.</summary>
    private string[] StepNames =>
        Program != null && Lookups.ProgramServices.TryGetValue(Program, out string[]? names) ? names : [Program ?? ""];

    /// <summary>
    /// The scheduled date of each step: the first one in mid-April
    /// (this year if sold before April, otherwise next year), then every 6 weeks.
    /// </summary>
    private DateTime StepDate(int index)
    {
        int year = DateSold.Month < 4 ? DateSold.Year : DateSold.Year + 1;
        return new DateTime(year, 4, 15).AddDays(42 * index);
    }

    /// <summary>Recalculates the service amount and the list of steps.</summary>
    private void UpdateSteps()
    {
        int.TryParse(MeasurementText, out int squareFeet);
        decimal total = StepNames.Sum(name => PriceList.StandardPrice(name, squareFeet));
        AmountText = total.ToString("0.00");
        Steps = StepNames.Select((name, i) => $"{name}  -  {Formats.Date(StepDate(i))}").ToList();
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
        {
            StatusMessage = $"{Program} added ({StepNames.Length} services). You can add another program or close.";
            InvoiceNumber = FakeData.NewInvoiceNumber();   // ready for the next program
        }
    }

    [RelayCommand]
    private void Cancel() => Close(false);

    /// <summary>Creates one service per program step. Returns false if the input is not valid.</summary>
    private bool Save()
    {
        ErrorMessage = "";
        if (string.IsNullOrWhiteSpace(Program))
        {
            ErrorMessage = "Choose a program.";
            return false;
        }
        if (!decimal.TryParse(AmountText, System.Globalization.NumberStyles.Currency, Formats.Us, out decimal total) || total < 0)
        {
            ErrorMessage = "Service amount must be a number, for example 260.00.";
            return false;
        }

        string[] names = StepNames;
        decimal each = Math.Round(total / names.Length, 2);
        for (int i = 0; i < names.Length; i++)
        {
            _account.Services.Add(new ServiceRecord
            {
                ServiceId = FakeData.NewServiceId(),
                // The first step uses the invoice number shown in the dialog.
                InvoiceNumber = i == 0 ? InvoiceNumber : FakeData.NewInvoiceNumber(),
                ServiceName = names[i],
                ProgramName = Program,
                DateSold = DateSold,
                OkayedBy = OkayedBy,
                DateScheduled = StepDate(i),
                Route = Route ?? "",
                Frequency = Frequency ?? "",
                Sort = Sort ?? "",
                Salesperson = Salesperson ?? "",
                PrenotifyType = _account.PrenotifyType,
                SaleAmount = each,
                Status = Status,
            });
        }
        _account.Program = Program;
        _account.EditedDate = FakeData.Today;
        _account.EditedBy = Session.UserName;
        return true;
    }
}
