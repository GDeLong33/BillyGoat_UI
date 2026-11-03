using System.Windows;

namespace BillyGoat_UI.Services;

/// <summary>
/// Shows simple message boxes. ViewModels call this instead of MessageBox directly,
/// so all "coming soon" popups look the same and are easy to replace later.
/// </summary>
public static class MessageService
{
    public static void Show(string message)
    {
        Window? owner = Application.Current?.MainWindow;
        if (owner != null)
        {
            MessageBox.Show(owner, message, "Billy Goat Lawn Care", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            MessageBox.Show(message, "Billy Goat Lawn Care", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
