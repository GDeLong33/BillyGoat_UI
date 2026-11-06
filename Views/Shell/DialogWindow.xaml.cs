using System.Windows;
using System.Windows.Input;

namespace BillyGoat_UI.Views;

/// <summary>The window used for every dialog. Opened by Services/DialogService.cs.</summary>
public partial class DialogWindow : Window
{
    public DialogWindow()
    {
        InitializeComponent();

        // Never taller than the screen: the content scrolls instead.
        MaxHeight = SystemParameters.WorkArea.Height - 20;
        SizeChanged += (_, _) => KeepOnScreen();
    }

    private void DialogWindow_Loaded(object sender, RoutedEventArgs e)
    {
        KeepOnScreen();

        // Put keyboard focus on the first input, so the user can start typing straight away.
        MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
    }

    /// <summary>
    /// Dialogs grow when a section appears (e.g. "Bill to another address").
    /// If the bottom would go below the screen, move the window up.
    /// </summary>
    private void KeepOnScreen()
    {
        Rect area = SystemParameters.WorkArea;
        if (Top + ActualHeight > area.Bottom)
            Top = Math.Max(area.Top, area.Bottom - ActualHeight);
    }
}
