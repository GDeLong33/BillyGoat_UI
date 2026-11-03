using System.Collections.ObjectModel;
using BillyGoat_UI.Models;
using BillyGoat_UI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BillyGoat_UI.ViewModels;

/// <summary>
/// ViewModel for the Employees page: the employee list with its filter,
/// the Actions panel, and the "Selected: ..." details panel.
/// </summary>
public partial class EmployeesViewModel : ObservableObject
{
    private readonly List<Employee> _allEmployees = FakeData.GetEmployees();

    // Called by the Exit button to go back to the Welcome tab.
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
        MessageService.Show($"Employee profile for {SelectedEmployee!.DisplayName} is coming soon.");

    [RelayCommand]
    private void Add() => MessageService.Show("Adding a new employee is coming soon.");

    /// <summary>Exit goes back to the Welcome page.</summary>
    [RelayCommand]
    private void Exit() => _goHome();

    [RelayCommand]
    private void TimeCards() => MessageService.Show("Employee time cards are coming soon.");

    [RelayCommand]
    private void Report() => MessageService.Show("The employee report is coming soon.");
}
