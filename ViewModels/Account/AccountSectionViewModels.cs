using BillyGoat_UI.Models;
using BillyGoat_UI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BillyGoat_UI.ViewModels;

// The simpler account sub-screens. Their views bind straight to the Account's
// properties, so these ViewModels are small. The bigger ones (Details, Services)
// have their own files.

/// <summary>1. Contact Information: billing address, service address and phones.</summary>
public class ContactInfoViewModel
{
    public ContactInfoViewModel(Account account) => Account = account;
    public Account Account { get; }
}

/// <summary>3. Pages and Notes: time-stamped office notes and special instructions.</summary>
public partial class NotesViewModel : ObservableObject
{
    public NotesViewModel(Account account)
    {
        Account = account;
        NewNoteText = "";
    }

    public Account Account { get; }

    /// <summary>Text in the "Add a note" box.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddNoteCommand))]
    public partial string NewNoteText { get; set; }

    private bool CanAddNote() => NewNoteText.Trim().Length > 0;

    /// <summary>Adds the note at the top of the list with today's date, the time and the user's name.</summary>
    [RelayCommand(CanExecute = nameof(CanAddNote))]
    private void AddNote()
    {
        Account.Notes.Insert(0, new OfficeNote { Timestamp = FakeData.Now, Text = NewNoteText.Trim(), EnteredBy = Session.UserName });
        NewNoteText = "";
    }
}

/// <summary>5. Accounting Defaults: statement, card, billing type and pay type settings.</summary>
public class AccountingDefaultsViewModel
{
    public AccountingDefaultsViewModel(Account account) => Account = account;
    public Account Account { get; }
}

/// <summary>6. Accounting Adjustments: the account's money history. Selecting a line shows its service below.</summary>
public partial class AdjustmentsViewModel : ObservableObject
{
    public AdjustmentsViewModel(Account account)
    {
        Account = account;
        SelectedEntry = account.Adjustments.LastOrDefault();
    }

    public Account Account { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedService))]
    public partial AdjustmentEntry? SelectedEntry { get; set; }

    /// <summary>The service of the selected line (shown in the details panel), or null.</summary>
    public ServiceRecord? SelectedService => SelectedEntry?.Service;
}

/// <summary>
/// 7. Marketing Letters: prepay percent, renewal letter, and the letters sent.
/// When prepay letters are printed on the Office tab, a record is added here automatically.
/// </summary>
public class MarketingLettersViewModel
{
    public MarketingLettersViewModel(Account account) => Account = account;
    public Account Account { get; }
}
