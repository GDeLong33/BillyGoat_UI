using System.Windows.Controls;
using System.Windows.Input;
using BillyGoat_UI.ViewModels;

namespace BillyGoat_UI.Views;

/// <summary>
/// Welcome page. The only code here forwards list events to the ViewModel,
/// because a DataGrid has no built-in "row double-clicked" command.
/// </summary>
public partial class WelcomeView : UserControl
{
    public WelcomeView()
    {
        InitializeComponent();
    }

    // Double-clicking a row opens that customer.
    private void CustomerRow_DoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is DataGridRow row && DataContext is WelcomeViewModel vm)
        {
            vm.OpenCustomerCommand.Execute(row.Item);
        }
    }

    // Keyboard users: pressing Enter on a row does the same as double-clicking it.
    private void CustomerGrid_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is WelcomeViewModel vm)
        {
            vm.OpenCustomerCommand.Execute(vm.SelectedCustomer);
            e.Handled = true;
        }
    }
}
