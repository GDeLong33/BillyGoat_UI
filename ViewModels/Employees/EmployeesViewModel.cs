using System.Collections.ObjectModel;
using BillyGoat_UI.Models;
using BillyGoat_UI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BillyGoat_UI.ViewModels;

/// <summary>
/// ViewModel for the Employees page: the employee list with its filter,
/// the Actions panel, and the "Selected: ..." details panel.
/// "Employee Reports" shows the reports screen in place of the list.
/// </summary>
public partial class EmployeesViewModel : ObservableObject
{
    private readonly List<Employee> _allEmployees = FakeData.Employees;

    // Called by the Exit button to go back to the Search tab.
    // MainViewModel passes this in, so this page does not need to know about MainViewModel.
    private readonly Action _goHome;

    public EmployeesViewModel(Action goHome)
    {
        _goHome = goHome;
        StatusFilter = EmployeeStatusFilter.Active;
        ApplyFilter();
    }

    /// <summary>The employees currently shown in the list.</summary>
    public ObservableCollection<Employee> Employees { get; } = new();

    /// <summary>All / Active / Terminated (bound to the radio row under the list).</summary>
    [ObservableProperty]
    public partial EmployeeStatusFilter StatusFilter { get; set; }

    /// <summary>The highlighted employee. The details panel shows this person.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedHeader))]
    [NotifyPropertyChangedFor(nameof(HasSelection))]
    [NotifyCanExecuteChangedFor(nameof(OpenCommand))]
    public partial Employee? SelectedEmployee { get; set; }

    /// <summary>Header of the details panel, e.g. "Selected: Alvarez, Marco".</summary>
    public string SelectedHeader =>
        SelectedEmployee != null ? $"Selected: {SelectedEmployee.DisplayName}" : "No employee selected";

    /// <summary>True when a row is selected (used to show/hide the details).</summary>
    public bool HasSelection => SelectedEmployee != null;

    /// <summary>The Employee Reports screen when it is open, otherwise null (the list shows).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsListVisible))]
    public partial EmployeeReportsViewModel? Reports { get; set; }

    public bool IsListVisible => Reports == null;

    partial void OnStatusFilterChanged(EmployeeStatusFilter value) => ApplyFilter();

    /// <summary>Rebuilds the visible list using the All / Active / Terminated filter.</summary>
    private void ApplyFilter()
    {
        IEnumerable<Employee> results = StatusFilter switch
        {
            EmployeeStatusFilter.Active => _allEmployees.Where(e => e.IsActive),
            EmployeeStatusFilter.Terminated => _allEmployees.Where(e => !e.IsActive),
            _ => _allEmployees,
        };

        Employee? previous = SelectedEmployee;

        Employees.Clear();
        foreach (Employee e in results)
            Employees.Add(e);

        // Keep the same person selected if still listed, otherwise select the first one.
        SelectedEmployee = previous != null && Employees.Contains(previous)
            ? previous
            : Employees.FirstOrDefault();
    }

    // ===== Actions =====

    /// <summary>Open button, double-click or Enter on a row.</summary>
    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void Open() =>
        MessageService.Show($"The employee record for {SelectedEmployee!.DisplayName} would open here.\n\n(Employee records are not part of this demo.)");

    [RelayCommand]
    private void Add() => MessageService.Show("A new, empty employee record would open here.\n\n(Employee records are not part of this demo.)");

    /// <summary>Exit goes back to the Search page.</summary>
    [RelayCommand]
    private void Exit() => _goHome();

    /// <summary>"Employee Reports" button: shows the reports screen; its Close button comes back here.</summary>
    [RelayCommand]
    private void OpenReports() => Reports = new EmployeeReportsViewModel(close: () => Reports = null);
}
