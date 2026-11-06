using System.Windows;

namespace BillyGoat_UI.Views;

/// <summary>
/// The main window: left tab rail + white main panel. All logic lives in MainViewModel.
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // The design size is 1440 x 960. On a smaller screen (e.g. a laptop),
        // shrink the window so it fits on screen, but never below the minimum size.
        Rect workArea = SystemParameters.WorkArea;
        if (Width > workArea.Width) Width = Math.Max(MinWidth, workArea.Width);
        if (Height > workArea.Height) Height = Math.Max(MinHeight, workArea.Height);
    }
}
