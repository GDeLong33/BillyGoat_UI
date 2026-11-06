using System.Windows;

namespace BillyGoat_UI.Services;

/// <summary>
/// Shows simple message boxes. ViewModels call this instead of MessageBox directly,
/// so all pop-ups look the same and are easy to replace later.
/// </summary>
public static class MessageService
{
    private const string Caption = "Billy Goat Lawn Care";

    /// <summary>An information message with an OK button.</summary>
    public static void Show(string message)
    {
        MessageBox.Show(GetOwner(), message, Caption, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    /// <summary>A warning message with an OK button (e.g. "Enter an amount first").</summary>
    public static void Warn(string message)
    {
        MessageBox.Show(GetOwner(), message, Caption, MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    /// <summary>A Yes / No question. Returns true when the user clicks Yes.</summary>
    public static bool Confirm(string question)
    {
        return MessageBox.Show(GetOwner(), question, Caption, MessageBoxButton.YesNo, MessageBoxImage.Question)
               == MessageBoxResult.Yes;
    }

    /// <summary>
    /// The window the message box belongs to: the active dialog if one is open,
    /// otherwise the main window. This keeps the message box on top of the right window.
    /// </summary>
    public static Window GetOwner()
    {
        Window? active = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
        return active ?? Application.Current.MainWindow;
    }
}
