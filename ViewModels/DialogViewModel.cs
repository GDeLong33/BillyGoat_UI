using CommunityToolkit.Mvvm.ComponentModel;

namespace BillyGoat_UI.ViewModels;

/// <summary>
/// Base class for every pop-up dialog (Add Service, Status Change, ...).
///
/// How dialogs work:
///   1. A ViewModel creates the dialog's ViewModel and calls DialogService.ShowDialog(vm).
///   2. DialogService opens a DialogWindow whose content is the ViewModel; the
///      DataTemplate in Resources/ViewTemplates.xaml picks the matching view.
///   3. When the user clicks OK or Cancel, the dialog's ViewModel calls Close(true/false).
///   4. ShowDialog returns that true/false to the caller.
/// So dialog ViewModels never touch Window objects and stay easy to test.
/// </summary>
public abstract class DialogViewModel : ObservableObject
{
    /// <summary>Text in the dialog's title bar.</summary>
    public abstract string Title { get; }

    /// <summary>Raised by Close(). DialogService listens for it and closes the window.</summary>
    public event Action<bool>? CloseRequested;

    /// <summary>Closes the dialog. true = OK / Finish, false = Cancel.</summary>
    protected void Close(bool result) => CloseRequested?.Invoke(result);
}
