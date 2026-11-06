using BillyGoat_UI.Models;
using BillyGoat_UI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BillyGoat_UI.ViewModels;

/// <summary>
/// Employee Reports screen (opened from the Employees tab): a list of reports,
/// a date range, and View (print preview) / Print (Windows print dialog) / Close.
/// </summary>
public partial class EmployeeReportsViewModel : ObservableObject
{
    private readonly Action _close;

    public EmployeeReportsViewModel(Action close)
    {
        _close = close;
        DateRanges.TryGet("This week", FakeData.Today, out DateTime from, out DateTime to);
        FromDate = from;
        ToDate = to;
        SelectedReport = Reports[0];
    }

    public List<ReportDefinition> Reports => ReportFactory.EmployeeReports;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ViewCommand), nameof(PrintCommand))]
    public partial ReportDefinition? SelectedReport { get; set; }

    [ObservableProperty] public partial DateTime? FromDate { get; set; }
    [ObservableProperty] public partial DateTime? ToDate { get; set; }

    private bool HasReport() => SelectedReport != null;

    /// <summary>View: opens the sample report in the print preview.</summary>
    [RelayCommand(CanExecute = nameof(HasReport))]
    private void View()
    {
        PrintService.ShowPreview(Build(), SelectedReport!.Name);
    }

    /// <summary>Print: straight to the Windows print dialog.</summary>
    [RelayCommand(CanExecute = nameof(HasReport))]
    private void Print()
    {
        PrintService.Print(Build(), SelectedReport!.Name);
    }

    private System.Windows.Documents.FlowDocument Build() =>
        ReportFactory.Report(SelectedReport!.Name, FromDate ?? FakeData.Today, ToDate ?? FakeData.Today);

    [RelayCommand]
    private void Close() => _close();
}
