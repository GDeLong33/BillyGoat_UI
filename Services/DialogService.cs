using BillyGoat_UI.ViewModels;
using BillyGoat_UI.Views;

namespace BillyGoat_UI.Services;

/// <summary>Opens a dialog for a DialogViewModel (see DialogViewModel for how this works).</summary>
public static class DialogService
{
    /// <summary>Shows the dialog and waits until it closes. Returns true for OK / Finish.</summary>
    public static bool ShowDialog(DialogViewModel viewModel)
    {
        DialogWindow window = new()
        {
            DataContext = viewModel,
            Title = viewModel.Title,
            Owner = MessageService.GetOwner(),
        };

        // When the ViewModel asks to close, setting DialogResult closes the window.
        // (IsVisible check: the window may already be closing, e.g. after the X button.)
        viewModel.CloseRequested += result =>
        {
            if (window.IsVisible)
                window.DialogResult = result;
        };

        return window.ShowDialog() == true;
    }
}
