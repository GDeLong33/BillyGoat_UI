using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BillyGoat_UI.Views;

/// <summary>
/// Adds "open the item" to any DataGrid, without code-behind in every view:
///     &lt;DataGrid views:DataGridOpen.Command="{Binding OpenAccountCommand}" ... /&gt;
///
/// The command runs (with the row's item as its parameter) when the user
///   - double-clicks a row, or
///   - presses Enter on the selected row (keyboard users).
///
/// This is an "attached property": a property defined here that can be set on
/// another control. When it is set, we hook up the DataGrid's events.
/// </summary>
public static class DataGridOpen
{
    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(DataGridOpen),
            new PropertyMetadata(null, OnCommandChanged));

    public static ICommand? GetCommand(DependencyObject element) => (ICommand?)element.GetValue(CommandProperty);
    public static void SetCommand(DependencyObject element, ICommand? value) => element.SetValue(CommandProperty, value);

    // Runs when the Command property is set in XAML: hook up the events once.
    private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DataGrid grid && e.OldValue == null)
        {
            grid.MouseDoubleClick += Grid_MouseDoubleClick;
            grid.PreviewKeyDown += Grid_PreviewKeyDown;
        }
    }

    private static void Grid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        // Only react to double-clicks on a row (not on the header or the scroll bar).
        DataGridRow? row = FindParentRow(e.OriginalSource as DependencyObject);
        if (row != null && sender is DataGrid grid)
        {
            Run(grid, row.Item);
            e.Handled = true;
        }
    }

    private static void Grid_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && sender is DataGrid grid && grid.SelectedItem != null)
        {
            Run(grid, grid.SelectedItem);
            e.Handled = true;   // stop the DataGrid from moving down a row
        }
    }

    private static void Run(DataGrid grid, object item)
    {
        ICommand? command = GetCommand(grid);
        if (command != null && command.CanExecute(item))
            command.Execute(item);
    }

    /// <summary>Walks up from the clicked element until it finds the DataGridRow it is in.</summary>
    private static DataGridRow? FindParentRow(DependencyObject? element)
    {
        while (element != null && element is not DataGridRow)
        {
            element = element is Visual
                ? VisualTreeHelper.GetParent(element)
                : LogicalTreeHelper.GetParent(element);   // e.g. a Run of text inside a cell
        }
        return element as DataGridRow;
    }
}
