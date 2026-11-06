using System.Collections.ObjectModel;
using BillyGoat_UI.Models;
using BillyGoat_UI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BillyGoat_UI.ViewModels;

/// <summary>
/// Routing tab. Left: the routes (with rename). Right: the selected route's stops in
/// driving order, with tools to reorder, add, remove, reschedule and print.
/// A property can be on more than one route; the "Other routes" column shows where else it is.
/// </summary>
public partial class RoutingViewModel : ObservableObject
{
    public RoutingViewModel()
    {
        RenameText = "";
        SelectedRoute = Routes.FirstOrDefault();
    }

    /// <summary>All routes (the same list the Routes drop-downs are built from).</summary>
    public ObservableCollection<Route> Routes => FakeData.Routes;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StopCountText))]
    [NotifyCanExecuteChangedFor(nameof(RenameCommand), nameof(AddCustomerCommand), nameof(SuggestOrderCommand),
                                nameof(MoveWorkCommand), nameof(PrintRouteCommand))]
    public partial Route? SelectedRoute { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(MoveUpCommand), nameof(MoveDownCommand), nameof(RemoveCommand))]
    public partial RouteStop? SelectedStop { get; set; }

    /// <summary>Text in the "Route name" box (for renaming).</summary>
    [ObservableProperty] public partial string RenameText { get; set; }

    public string StopCountText => SelectedRoute == null ? "" :
        $"{SelectedRoute.Stops.Count} stops, {SelectedRoute.Stops.Count(s => !s.IsCompleted)} not completed";

    partial void OnSelectedRouteChanged(Route? value)
    {
        RenameText = value?.Name ?? "";
        SelectedStop = value?.Stops.FirstOrDefault();
        RefreshOtherRoutes();
    }

    /// <summary>Fills in each stop's "Other routes" column.</summary>
    private void RefreshOtherRoutes()
    {
        if (SelectedRoute == null) return;
        foreach (RouteStop stop in SelectedRoute.Stops)
        {
            stop.OtherRoutes = string.Join(", ", Routes
                .Where(r => r != SelectedRoute && r.Stops.Any(s => s.Account == stop.Account))
                .Select(r => r.DisplayName));
        }
        OnPropertyChanged(nameof(StopCountText));
    }

    private bool HasRoute() => SelectedRoute != null;
    private bool HasStop() => SelectedStop != null;

    // ===== Route list =====

    /// <summary>Rename: changes the route name everywhere (drop-down lists, accounts, services).</summary>
    [RelayCommand(CanExecute = nameof(HasRoute))]
    private void Rename()
    {
        string name = RenameText.Trim();
        if (name.Length == 0)
        {
            MessageService.Warn("Type a name for the route.");
            return;
        }
        FakeData.RenameRoute(SelectedRoute!, name);
        RefreshOtherRoutes();
    }

    // ===== Stops =====

    [RelayCommand(CanExecute = nameof(HasStop))]
    private void MoveUp() => MoveSelected(-1);

    [RelayCommand(CanExecute = nameof(HasStop))]
    private void MoveDown() => MoveSelected(+1);

    /// <summary>Moves the selected stop one place up (-1) or down (+1) and renumbers the stops.</summary>
    private void MoveSelected(int direction)
    {
        Route route = SelectedRoute!;
        RouteStop stop = SelectedStop!;
        int index = route.Stops.IndexOf(stop);
        int newIndex = index + direction;
        if (newIndex < 0 || newIndex >= route.Stops.Count)
            return;

        route.Stops.Move(index, newIndex);
        route.RenumberStops();
        SelectedStop = stop;   // keep it selected so it can be moved again
    }

    /// <summary>"Add customer to route": choose an account, a service and a date.</summary>
    [RelayCommand(CanExecute = nameof(HasRoute))]
    private void AddCustomer()
    {
        AddStopViewModel dialog = new(SelectedRoute!);
        if (DialogService.ShowDialog(dialog))
        {
            RouteStop stop = new(dialog.SelectedAccount!, dialog.ServiceName!, dialog.ScheduledDate);
            SelectedRoute!.Stops.Add(stop);
            SelectedRoute.RenumberStops();
            SelectedStop = stop;
            RefreshOtherRoutes();
        }
    }

    [RelayCommand(CanExecute = nameof(HasStop))]
    private void Remove()
    {
        RouteStop stop = SelectedStop!;
        if (!MessageService.Confirm($"Remove {stop.CustomerName} ({stop.ServiceName}) from {SelectedRoute!.DisplayName}?"))
            return;
        SelectedRoute.Stops.Remove(stop);
        SelectedRoute.RenumberStops();
        SelectedStop = SelectedRoute.Stops.FirstOrDefault();
        RefreshOtherRoutes();
    }

    /// <summary>
    /// "Move uncompleted work to a later date" (e.g. after a rain day): every stop not
    /// completed gets the new date, and so do the matching services on the accounts.
    /// </summary>
    [RelayCommand(CanExecute = nameof(HasRoute))]
    private void MoveWork()
    {
        var notDone = SelectedRoute!.Stops.Where(s => !s.IsCompleted).ToList();
        if (notDone.Count == 0)
        {
            MessageService.Show("Every stop on this route is completed.");
            return;
        }

        PickDateViewModel dialog = new("Move Uncompleted Work",
            $"Move {notDone.Count} uncompleted stop(s) on {SelectedRoute.DisplayName} to:", FakeData.Today.AddDays(1));
        if (!DialogService.ShowDialog(dialog))
            return;

        foreach (RouteStop stop in notDone)
        {
            stop.ScheduledDate = dialog.Date;
            foreach (ServiceRecord service in stop.Account.Services.Where(s => s.ServiceName == stop.ServiceName && s.IsOutstanding))
                service.DateScheduled = dialog.Date;
        }
        MessageService.Show($"{notDone.Count} stop(s) moved to {Formats.Date(dialog.Date)}.");
    }

    /// <summary>
    /// "Suggest stop order": the real program would send the stop addresses to a
    /// mapping service (for example Google Maps or Bing Maps) and ask for the shortest
    /// driving order. The demo has no network, so it just sorts by zip code and street.
    /// </summary>
    [RelayCommand(CanExecute = nameof(HasRoute))]
    private void SuggestOrder()
    {
        Route route = SelectedRoute!;
        if (!MessageService.Confirm(
                $"This would send the {route.Stops.Count} stop addresses to a mapping service to find the shortest driving order.\n\n" +
                "Demo: no mapping service is called; the stops are sorted by zip code and street instead.\n\nReorder the stops now?"))
            return;

        var suggested = route.Stops.OrderBy(s => s.Account.Service.Zip).ThenBy(s => s.Account.Service.StreetName).ToList();
        route.Stops.Clear();
        foreach (RouteStop stop in suggested)
            route.Stops.Add(stop);
        route.RenumberStops();
        SelectedStop = route.Stops.FirstOrDefault();
        MessageService.Show("Stops reordered. Use Move up / Move down to adjust the order.");
    }

    /// <summary>"Print all work orders for this route" in stop order.</summary>
    [RelayCommand(CanExecute = nameof(HasRoute))]
    private void PrintRoute()
    {
        // For each stop not yet completed, find the account's matching open service.
        var orders = new List<(Account Account, ServiceRecord Service)>();
        foreach (RouteStop stop in SelectedRoute!.Stops.Where(s => !s.IsCompleted))
        {
            ServiceRecord? service = stop.Account.Services
                .Where(s => s.ServiceName == stop.ServiceName && s.IsOutstanding)
                .OrderBy(s => s.DateScheduled).FirstOrDefault();
            if (service != null)
                orders.Add((stop.Account, service));
        }
        if (orders.Count == 0)
        {
            MessageService.Show("There are no open work orders on this route.");
            return;
        }

        var document = ReportFactory.WorkOrders(orders, $"{SelectedRoute.DisplayName}, {orders.Count} stops in route order");
        if (PrintService.PreviewThenAsk(document, "Route work orders", $"Mark all {orders.Count} work orders on this route as printed?"))
        {
            foreach ((_, ServiceRecord service) in orders)
            {
                service.Status = ServiceRecord.Printed;
                service.DatePrinted = FakeData.Today;
            }
        }
    }
}
