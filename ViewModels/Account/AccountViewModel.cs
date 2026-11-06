using BillyGoat_UI.Models;
using BillyGoat_UI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BillyGoat_UI.ViewModels;

/// <summary>The sub-screens of an open account, in the order of the sub-menu.</summary>
public enum AccountSection
{
    ContactInformation,
    Details,
    PagesAndNotes,
    Services,
    AccountingDefaults,
    AccountingAdjustments,
    MarketingLetters
}

/// <summary>
/// ViewModel for the Account screen (opened by double-clicking a search result).
/// It holds the open account and switches between its sub-screens the same way
/// MainViewModel switches tabs: SelectedSection -> CurrentSection -> DataTemplate.
/// </summary>
public partial class AccountViewModel : ObservableObject
{
    private readonly Action _backToMenu;
    private readonly Action _addNewCustomer;

    // One ViewModel per sub-screen, created the first time it is shown.
    private readonly Dictionary<AccountSection, object> _sections = new();

    public AccountViewModel(Account account, Action backToMenu, Action addNewCustomer)
    {
        Account = account;
        _backToMenu = backToMenu;
        _addNewCustomer = addNewCustomer;

        SelectedSection = AccountSection.ContactInformation;
        CurrentSection = GetSection(AccountSection.ContactInformation);
    }

    /// <summary>The open account. The header binds to its number, name and status.</summary>
    public Account Account { get; }

    /// <summary>The sub-menu item that is selected.</summary>
    [ObservableProperty]
    public partial AccountSection SelectedSection { get; set; }

    /// <summary>The ViewModel of the sub-screen shown on the right.</summary>
    [ObservableProperty]
    public partial object? CurrentSection { get; set; }

    partial void OnSelectedSectionChanged(AccountSection value)
    {
        CurrentSection = GetSection(value);
        Account.RefreshDisplay();   // the header name may have been edited on Contact Information
    }

    /// <summary>Returns the sub-screen's ViewModel, creating it the first time.</summary>
    private object GetSection(AccountSection section)
    {
        if (!_sections.TryGetValue(section, out object? vm))
        {
            vm = CreateSection(section);
            _sections[section] = vm;
        }
        return vm;
    }

    private object CreateSection(AccountSection section) => section switch
    {
        AccountSection.ContactInformation => new PlaceholderViewModel("Contact Information"),
        AccountSection.Details => new PlaceholderViewModel("Details"),
        AccountSection.PagesAndNotes => new PlaceholderViewModel("Pages and Notes"),
        AccountSection.Services => new PlaceholderViewModel("Services"),
        AccountSection.AccountingDefaults => new PlaceholderViewModel("Accounting Defaults"),
        AccountSection.AccountingAdjustments => new PlaceholderViewModel("Accounting Adjustments"),
        _ => new PlaceholderViewModel("Marketing Letters"),
    };

    // ===== Commands =====

    /// <summary>"Back to main menu" button.</summary>
    [RelayCommand]
    private void BackToMenu() => _backToMenu();

    /// <summary>"Add new customer" under the sub-menu.</summary>
    [RelayCommand]
    private void AddNewCustomer() => _addNewCustomer();

    /// <summary>Menu: Accounting > Enter a payment.</summary>
    [RelayCommand]
    private void EnterPayment() => MessageService.Show("Enter a payment is coming soon.");
}
