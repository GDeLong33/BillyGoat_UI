using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BillyGoat_UI.Models;

/// <summary>
/// A route: an ordered list of stops a crew drives in one day.
/// The same property can be on more than one route (e.g. a mowing route and a
/// fertilization route), because each stop is one service at one account.
/// </summary>
public partial class Route : ObservableObject
{
    public Route(int number, string name)
    {
        Number = number;
        Name = name;
    }

    public int Number { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayName))]
    public partial string Name { get; set; }

    /// <summary>"1 - Saginaw North". This is also the text used in the Routes drop-down lists.</summary>
    public string DisplayName => $"{Number} - {Name}";

    /// <summary>The name is what screen readers announce for the list item.</summary>
    public override string ToString() => DisplayName;

    /// <summary>The stops in driving order.</summary>
    public ObservableCollection<RouteStop> Stops { get; } = new();

    /// <summary>Sets each stop's StopNumber to 1, 2, 3... after the order changes.</summary>
    public void RenumberStops()
    {
        for (int i = 0; i < Stops.Count; i++)
            Stops[i].StopNumber = i + 1;
    }
}

/// <summary>One stop on a route: which account and which service is done there.</summary>
public partial class RouteStop : ObservableObject
{
    public RouteStop(Account account, string serviceName, DateTime scheduledDate)
    {
        Account = account;
        ServiceName = serviceName;
        ScheduledDate = scheduledDate;
    }

    public Account Account { get; }
    public string ServiceName { get; }

    [ObservableProperty]
    public partial int StopNumber { get; set; }

    [ObservableProperty]
    public partial DateTime ScheduledDate { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DoneText))]
    public partial bool IsCompleted { get; set; }

    /// <summary>"Done" or empty, for the Done column.</summary>
    public string DoneText => IsCompleted ? "Done" : "";

    /// <summary>Text shown in the "Other routes" column (filled in by RoutingViewModel).</summary>
    [ObservableProperty]
    public partial string OtherRoutes { get; set; } = "";

    public string CustomerName => Account.Service.DisplayName;
    public string ServiceAddress => Account.Service.OneLine;
}
