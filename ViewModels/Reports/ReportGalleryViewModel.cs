using BillyGoat_UI.Models;
using BillyGoat_UI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BillyGoat_UI.ViewModels;

/// <summary>
/// Report Gallery tab: pick a report on the left, set its filters on the right,
/// and Preview opens the print preview with sample data.
/// </summary>
public partial class ReportGalleryViewModel : ObservableObject
{
    private const string AllServices = "All services";

    public ReportGalleryViewModel()
    {
        ServiceChoices = [AllServices, .. Lookups.Services];
        Service = AllServices;
        QuickDate = "This year";   // also sets FromDate and ToDate
        SelectedReport = Reports[0];
    }

    public List<ReportDefinition> Reports => ReportFactory.GalleryReports;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowServiceFilter))]
    [NotifyCanExecuteChangedFor(nameof(PreviewCommand))]
    public partial ReportDefinition? SelectedReport { get; set; }

    /// <summary>Only some reports can be limited to one service.</summary>
    public bool ShowServiceFilter => SelectedReport?.UsesService == true;

    // ===== Filters =====

    public string[] QuickDates => Lookups.QuickDates;

    /// <summary>Today / This week / 2 weeks / This month ... Picking one fills in From and To.</summary>
    [ObservableProperty] public partial string QuickDate { get; set; }
    [ObservableProperty] public partial DateTime? FromDate { get; set; }
    [ObservableProperty] public partial DateTime? ToDate { get; set; }

    public List<string> ServiceChoices { get; }
    [ObservableProperty] public partial string Service { get; set; }

    partial void OnQuickDateChanged(string value)
    {
        if (DateRanges.TryGet(value, FakeData.Today, out DateTime from, out DateTime to))
        {
            FromDate = from;
            ToDate = to;
        }
    }

    private bool CanPreview() => SelectedReport != null;

    /// <summary>Preview button: builds the report and opens the print preview.</summary>
    [RelayCommand(CanExecute = nameof(CanPreview))]
    private void Preview()
    {
        DateTime from = FromDate ?? FakeData.Today;
        DateTime to = ToDate ?? FakeData.Today;
        if (from > to)
        {
            MessageService.Warn("The From date must be before the To date.");
            return;
        }
        string service = ShowServiceFilter ? Service : AllServices;
        PrintService.ShowPreview(ReportFactory.Report(SelectedReport!.Name, from, to, service), SelectedReport.Name);
    }
}

/// <summary>Turns a quick date choice ("This week", "This month"...) into a From and To date.</summary>
public static class DateRanges
{
    /// <summary>Returns false for "Custom" (keep the dates the user typed).</summary>
    public static bool TryGet(string quickDate, DateTime today, out DateTime from, out DateTime to)
    {
        // Weeks start on Monday.
        DateTime monday = today.AddDays(-(((int)today.DayOfWeek + 6) % 7));
        DateTime firstOfMonth = new(today.Year, today.Month, 1);

        (from, to) = quickDate switch
        {
            "Today" => (today, today),
            "This week" => (monday, monday.AddDays(6)),
            "2 weeks" => (today.AddDays(-13), today),
            "This month" => (firstOfMonth, firstOfMonth.AddMonths(1).AddDays(-1)),
            "Last month" => (firstOfMonth.AddMonths(-1), firstOfMonth.AddDays(-1)),
            "This year" => (new DateTime(today.Year, 1, 1), new DateTime(today.Year, 12, 31)),
            _ => (default, default),
        };
        return from != default;
    }
}
