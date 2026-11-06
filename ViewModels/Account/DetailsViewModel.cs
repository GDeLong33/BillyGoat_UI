using System.Windows;
using BillyGoat_UI.Models;
using BillyGoat_UI.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BillyGoat_UI.ViewModels;

/// <summary>
/// 2. Details: account details, service defaults and record information.
/// Most fields bind straight to the Account. The one special field is Account status:
/// changing it opens the Status Change dialog, and the change only happens on Finish.
/// </summary>
public partial class DetailsViewModel : ObservableObject
{
    public DetailsViewModel(Account account)
    {
        Account = account;
        SelectedStatus = account.Status;
    }

    public Account Account { get; }

    /// <summary>
    /// The value shown in the Account status drop-down. It is separate from
    /// Account.Status so we can put it back if the user cancels the dialog.
    /// </summary>
    [ObservableProperty]
    public partial string SelectedStatus { get; set; }

    partial void OnSelectedStatusChanged(string value)
    {
        if (value == Account.Status)
            return;

        // Open the dialog after the drop-down has finished closing
        // (opening a window in the middle of a selection change confuses the ComboBox).
        Application.Current.Dispatcher.BeginInvoke(() => ConfirmStatusChange(value));
    }

    private void ConfirmStatusChange(string newStatus)
    {
        StatusChangeViewModel dialog = new(Account, newStatus);
        if (!DialogService.ShowDialog(dialog))
        {
            // Cancelled: put the drop-down back to the current status.
            SelectedStatus = Account.Status;
        }
    }
}
