using System.Collections.ObjectModel;
using BillyGoat_UI.Models;
using BillyGoat_UI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BillyGoat_UI.ViewModels;

/// <summary>Which services the Services screen shows.</summary>
public enum ServiceViewOption
{
    ThisYear,
    LastYear,
    Future,
    All
}

/// <summary>
/// 4. Services: the account's services, filtered by year, with Print work order,
/// Add program and Add service. Double-click a service to edit it.
/// </summary>
public partial class ServicesViewModel : ObservableObject
{
    public ServicesViewModel(Account account)
    {
        Account = account;
        ViewOption = ServiceViewOption.ThisYear;
        Refresh();
    }

    public Account Account { get; }

    /// <summary>The services shown in the table.</summary>
    public ObservableCollection<ServiceRecord> Services { get; } = new();

    /// <summary>This year / Last year / Future / All (the view option bar).</summary>
    [ObservableProperty]
    public partial ServiceViewOption ViewOption { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PrintWorkOrderCommand))]
    public partial ServiceRecord? SelectedService { get; set; }

    /// <summary>"Balance for these services: $120.00"</summary>
    public string TotalsText =>
        $"{Services.Count} service(s)    Balance for these services: {Formats.Money(Services.Where(s => s.Status != ServiceRecord.Inactive).Sum(s => s.Balance))}";

    partial void OnViewOptionChanged(ServiceViewOption value) => Refresh();

    /// <summary>Rebuilds the table from the account's services using the view option.</summary>
    private void Refresh()
    {
        int year = FakeData.Today.Year;
        IEnumerable<ServiceRecord> list = ViewOption switch
        {
            ServiceViewOption.ThisYear => Account.Services.Where(s => s.DateScheduled.Year == year),
            ServiceViewOption.LastYear => Account.Services.Where(s => s.DateScheduled.Year == year - 1),
            ServiceViewOption.Future => Account.Services.Where(s => s.DateScheduled > FakeData.Today),
            _ => Account.Services,
        };

        ServiceRecord? previous = SelectedService;
        Services.Clear();
        foreach (ServiceRecord service in list.OrderBy(s => s.DateScheduled))
            Services.Add(service);
        SelectedService = previous != null && Services.Contains(previous) ? previous : Services.FirstOrDefault();
        OnPropertyChanged(nameof(TotalsText));
    }

    private bool HasSelection() => SelectedService != null;

    /// <summary>
    /// Print work order: print preview of the selected service; when the preview
    /// closes, ask whether to mark it as printed.
    /// </summary>
    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void PrintWorkOrder()
    {
        ServiceRecord service = SelectedService!;
        var document = ReportFactory.WorkOrders([(Account, service)], $"Account {Account.AccountNumber}");

        if (PrintService.PreviewThenAsk(document, "Work order", "Mark this work order as printed?"))
        {
            service.DatePrinted = FakeData.Today;
            if (service.Status == ServiceRecord.Open)
                service.Status = ServiceRecord.Printed;
        }
    }

    /// <summary>Add service button: opens the Add Service dialog.</summary>
    [RelayCommand]
    private void AddService()
    {
        DialogService.ShowDialog(new ServiceEditorViewModel(Account, existing: null));
        Refresh();   // Apply may have saved a service even if the dialog was then cancelled
    }

    /// <summary>Double-click (or Enter) on a service: opens Edit Service.</summary>
    [RelayCommand]
    private void EditService(ServiceRecord? service)
    {
        if (service == null) return;
        DialogService.ShowDialog(new ServiceEditorViewModel(Account, service));
        Refresh();
    }

    /// <summary>Add program button: opens the Add Program dialog.</summary>
    [RelayCommand]
    private void AddProgram()
    {
        DialogService.ShowDialog(new AddProgramViewModel(Account));
        Refresh();
    }
}
