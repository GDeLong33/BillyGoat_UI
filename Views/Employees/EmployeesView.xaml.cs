using System.Windows.Controls;
using System.Windows.Input;
using BillyGoat_UI.ViewModels;

namespace BillyGoat_UI.Views;

/// <summary>
/// Employees page. The only code here forwards list events to the ViewModel,
/// because a DataGrid has no built-in "row double-clicked" command.
/// </summary>
public partial class EmployeesView : UserControl
{
    public EmployeesView()
    {
        InitializeComponent();
    }

    // Double-clicking a row selects that employee and opens them.
    private void EmployeeRow_DoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is DataGridRow row && DataContext is EmployeesViewModel vm)
        {
            vm.SelectedEmployee = (Models.Employee)row.Item;
            vm.OpenCommand.Execute(null);
        }
    }

    // Keyboard users: Enter on a row does the same as double-clicking it.
    private void EmployeeGrid_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is EmployeesViewModel vm && vm.OpenCommand.CanExecute(null))
        {
            vm.OpenCommand.Execute(null);
            e.Handled = true;
        }
    }
}
