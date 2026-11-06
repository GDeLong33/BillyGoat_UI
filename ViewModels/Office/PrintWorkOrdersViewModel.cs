using BillyGoat_UI.Models;
using BillyGoat_UI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BillyGoat_UI.ViewModels;

/// <summary>How the found work orders are sorted (wizard step 3).</summary>
public enum WorkOrderSort { RouteThenDate, ScheduleDate, ZipCode, StreetName, CustomerName }

/// <summary>
/// Office tool 3: Print Work Orders, a 3-step wizard.
///   Step 1: which service and which programs.
///   Step 2: how to pick the orders (ranges: route, county, city, zip, street, date, priority).
///   Step 3: how to sort them.
/// Then: print preview of the open services found (active accounts only), and on close
/// "Mark all found work orders as printed?".
/// </summary>
public partial class PrintWorkOrdersViewModel : ObservableObject
{
    private const string AllServices = "All services";
    private const string NoProgram = "(Services not in a program)";
    private readonly Action _close;

    public PrintWorkOrdersViewModel(Action close)
    {
        _close = close;
        Step = 1;

        // Step 1
        ServiceChoices = [AllServices, .. Lookups.Services];
        ServiceToPrint = AllServices;
        Programs = Lookups.Programs.Select(p => new CheckItem(p, isChecked: true))
            .Append(new CheckItem(NoProgram, isChecked: true)).ToList();

        // Step 2 (empty "from" / "to" = no limit)
        RouteFrom = ""; RouteTo = ""; MaxPerRoute = "";
        CountyFrom = ""; CountyTo = ""; CityFrom = ""; CityTo = "";
        ZipFrom = ""; ZipTo = ""; StreetFrom = ""; StreetTo = "";
        PriorityFrom = ""; PriorityTo = "";
        ScheduleFrom = FakeData.Today;
        ScheduleTo = FakeData.Today.AddDays(7);

        // Step 3
        SortBy = WorkOrderSort.RouteThenDate;
        ResultText = "";
    }

    // ===== Wizard navigation =====

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsStep1), nameof(IsStep2), nameof(IsStep3), nameof(StepText), nameof(IsLastStep))]
    [NotifyCanExecuteChangedFor(nameof(BackCommand), nameof(NextCommand))]
    public partial int Step { get; set; }

    public bool IsStep1 => Step == 1;
    public bool IsStep2 => Step == 2;
    public bool IsStep3 => Step == 3;
    public bool IsLastStep => Step == 3;
    public string StepText => Step switch
    {
        1 => "Step 1 of 3: What to print",
        2 => "Step 2 of 3: How to pick the orders",
        _ => "Step 3 of 3: Sorting",
    };

    [RelayCommand(CanExecute = nameof(CanGoBack))]
    private void Back() => Step--;
    private bool CanGoBack() => Step > 1;

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private void Next() => Step++;
    private bool CanGoNext() => Step < 3;

    // ===== Step 1 =====
    public List<string> ServiceChoices { get; }
    [ObservableProperty] public partial string ServiceToPrint { get; set; }
    public List<CheckItem> Programs { get; }

    [RelayCommand]
    private void CheckAll() => Programs.ForEach(p => p.IsChecked = true);

    [RelayCommand]
    private void UncheckAll() => Programs.ForEach(p => p.IsChecked = false);

    // ===== Step 2 =====
    public List<string> RouteChoices { get; } = ["", .. Lookups.Routes];
    [ObservableProperty] public partial string RouteFrom { get; set; }
    [ObservableProperty] public partial string RouteTo { get; set; }
    [ObservableProperty] public partial string MaxPerRoute { get; set; }
    [ObservableProperty] public partial string CountyFrom { get; set; }
    [ObservableProperty] public partial string CountyTo { get; set; }
    [ObservableProperty] public partial string CityFrom { get; set; }
    [ObservableProperty] public partial string CityTo { get; set; }
    [ObservableProperty] public partial string ZipFrom { get; set; }
    [ObservableProperty] public partial string ZipTo { get; set; }
    [ObservableProperty] public partial string StreetFrom { get; set; }
    [ObservableProperty] public partial string StreetTo { get; set; }
    [ObservableProperty] public partial DateTime? ScheduleFrom { get; set; }
    [ObservableProperty] public partial DateTime? ScheduleTo { get; set; }
    [ObservableProperty] public partial string PriorityFrom { get; set; }
    [ObservableProperty] public partial string PriorityTo { get; set; }

    // ===== Step 3 =====
    [ObservableProperty] public partial WorkOrderSort SortBy { get; set; }
    [ObservableProperty] public partial string ResultText { get; set; }

    // ===== Find, preview and print =====

    /// <summary>"Find and preview" button.</summary>
    [RelayCommand]
    private void FindAndPreview()
    {
        var orders = FindOrders();
        if (orders.Count == 0)
        {
            ResultText = "";
            MessageService.Show("No open work orders match these choices. Try a wider date range or more programs.");
            return;
        }

        ResultText = $"{orders.Count} work order(s) found.";
        var document = ReportFactory.WorkOrders(orders, $"{orders.Count} work orders, {Formats.Date(ScheduleFrom)} to {Formats.Date(ScheduleTo)}");

        if (PrintService.PreviewThenAsk(document, "Work orders", $"Mark all {orders.Count} found work orders as printed?"))
        {
            foreach ((_, ServiceRecord service) in orders)
            {
                service.Status = ServiceRecord.Printed;
                service.DatePrinted = FakeData.Today;
            }
            ResultText = $"{orders.Count} work order(s) marked as printed.";
        }
    }

    /// <summary>Applies steps 1 to 3 to every open service of every active account.</summary>
    private List<(Account Account, ServiceRecord Service)> FindOrders()
    {
        var checkedPrograms = Programs.Where(p => p.IsChecked).Select(p => p.Name).ToHashSet();

        var found = new List<(Account Account, ServiceRecord Service)>();
        foreach (Account account in FakeData.Accounts.Where(a => a.IsActive))   // only active accounts
        {
            Address address = account.Service;
            foreach (ServiceRecord s in account.Services.Where(s => s.Status == ServiceRecord.Open))
            {
                // Step 1: service and program
                if (ServiceToPrint != AllServices && !s.ServiceName.StartsWith(ServiceToPrint, StringComparison.OrdinalIgnoreCase)) continue;
                string program = s.ProgramName.Length > 0 ? s.ProgramName : NoProgram;
                if (!checkedPrograms.Contains(program)) continue;

                // Step 2: ranges
                if (!InRange(s.Route, RouteFrom, RouteTo)) continue;
                if (!InRange(CountyOf(address.City), CountyFrom, CountyTo)) continue;
                if (!InRange(address.City, CityFrom, CityTo)) continue;
                if (!InRange(address.Zip, ZipFrom, ZipTo)) continue;
                if (!InRange(address.StreetName, StreetFrom, StreetTo)) continue;
                if (!InRange(s.Sort, PriorityFrom, PriorityTo)) continue;
                if (ScheduleFrom.HasValue && s.DateScheduled < ScheduleFrom.Value) continue;
                if (ScheduleTo.HasValue && s.DateScheduled > ScheduleTo.Value) continue;

                found.Add((account, s));
            }
        }

        // Step 3: sorting
        found = SortBy switch
        {
            WorkOrderSort.ScheduleDate => found.OrderBy(o => o.Service.DateScheduled).ToList(),
            WorkOrderSort.ZipCode => found.OrderBy(o => o.Account.Service.Zip).ToList(),
            WorkOrderSort.StreetName => found.OrderBy(o => o.Account.Service.StreetName).ToList(),
            WorkOrderSort.CustomerName => found.OrderBy(o => o.Account.Service.DisplayName).ToList(),
            _ => found.OrderBy(o => o.Service.Route).ThenBy(o => o.Service.DateScheduled).ToList(),
        };

        // "Amount per route": only the first N orders of each route.
        if (int.TryParse(MaxPerRoute, out int max) && max > 0)
            found = found.GroupBy(o => o.Service.Route).SelectMany(g => g.Take(max)).ToList();

        return found;
    }

    /// <summary>
    /// True when value is between from and to (alphabetical, not case sensitive).
    /// An empty "from" or "to" means no limit on that side.
    /// </summary>
    private static bool InRange(string value, string? from, string? to)
    {
        if (!string.IsNullOrWhiteSpace(from) && string.Compare(value, from.Trim(), StringComparison.OrdinalIgnoreCase) < 0) return false;
        if (!string.IsNullOrWhiteSpace(to) && string.Compare(value, to.Trim(), StringComparison.OrdinalIgnoreCase) > 0) return false;
        return true;
    }

    /// <summary>The demo's cities and their counties.</summary>
    private static string CountyOf(string city) => city switch
    {
        "Bay City" => "Bay",
        _ => "Saginaw",
    };

    [RelayCommand]
    private void Close() => _close();
}
