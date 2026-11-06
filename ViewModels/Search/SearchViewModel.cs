using System.Collections.ObjectModel;
using BillyGoat_UI.Models;
using BillyGoat_UI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BillyGoat_UI.ViewModels;

/// <summary>
/// ViewModel for the Search tab: find an account by last name, first name,
/// account number or street address, then double-click it to open it.
/// </summary>
public partial class SearchViewModel : ObservableObject
{
    // MainViewModel passes these in, so this page does not need to know about MainViewModel.
    private readonly Action<Account> _openAccount;
    private readonly Action _addNewCustomer;

    public SearchViewModel(Action<Account> openAccount, Action addNewCustomer)
    {
        _openAccount = openAccount;
        _addNewCustomer = addNewCustomer;
        SearchText = "";
        SearchField = AccountSearchField.LastName;
        StatusFilter = AccountStatusFilter.Active;
        Refresh();
    }

    /// <summary>The accounts currently shown in the results table.</summary>
    public ObservableCollection<Account> Results { get; } = new();

    /// <summary>Which field to search (bound to the "Search by" radio buttons).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SearchHint))]
    public partial AccountSearchField SearchField { get; set; }

    /// <summary>Text typed in the search box. The results filter as you type.</summary>
    [ObservableProperty]
    public partial string SearchText { get; set; }

    /// <summary>Active / All / Inactive (bound to the radio row under the table).</summary>
    [ObservableProperty]
    public partial AccountStatusFilter StatusFilter { get; set; }

    /// <summary>The highlighted row.</summary>
    [ObservableProperty]
    public partial Account? SelectedAccount { get; set; }

    /// <summary>"3 accounts found"</summary>
    public string ResultCountText => Results.Count == 1 ? "1 account found" : $"{Results.Count} accounts found";

    /// <summary>Grey hint text shown in the empty search box.</summary>
    public string SearchHint => SearchField switch
    {
        AccountSearchField.FirstName => "Type a first name",
        AccountSearchField.AccountNumber => "Type an account number, e.g. A-01842",
        AccountSearchField.StreetAddress => "Type a street address, e.g. Cedar",
        _ => "Type a last name",
    };

    // Any change to the search re-filters the list straight away.
    partial void OnSearchTextChanged(string value) => Refresh();
    partial void OnSearchFieldChanged(AccountSearchField value) => Refresh();
    partial void OnStatusFilterChanged(AccountStatusFilter value) => Refresh();

    /// <summary>Search button (or Enter in the search box).</summary>
    [RelayCommand]
    private void Search() => Refresh();

    /// <summary>Double-click or Enter on a row.</summary>
    [RelayCommand]
    private void OpenAccount(Account? account)
    {
        if (account != null)
            _openAccount(account);
    }

    /// <summary>"Add new customer" button.</summary>
    [RelayCommand]
    private void AddNewCustomer() => _addNewCustomer();

    /// <summary>
    /// Rebuilds the results from all accounts, using the status filter and the
    /// search text in the chosen field (case-insensitive "contains").
    /// Also called by MainViewModel when coming back from an account, so edits show up.
    /// </summary>
    public void Refresh()
    {
        string text = (SearchText ?? "").Trim();
        IEnumerable<Account> results = FakeData.Accounts;

        // 1. Active / All / Inactive
        if (StatusFilter == AccountStatusFilter.Active)
            results = results.Where(a => a.IsActive);
        else if (StatusFilter == AccountStatusFilter.Inactive)
            results = results.Where(a => !a.IsActive);

        // 2. Search text in the chosen field (empty text = show everything)
        if (text.Length > 0)
            results = results.Where(a => Matches(a, text));

        Account? previous = SelectedAccount;
        Results.Clear();
        foreach (Account account in results.OrderBy(a => a.BillingName))
        {
            account.RefreshDisplay();   // pick up any edits made on the Account screen
            Results.Add(account);
        }

        // Keep the same account selected if it is still in the results.
        SelectedAccount = previous != null && Results.Contains(previous) ? previous : Results.FirstOrDefault();
        OnPropertyChanged(nameof(ResultCountText));
    }

    /// <summary>
    /// True when the account matches the text in the chosen field. Names and
    /// streets are checked on both the billing and the service address.
    /// </summary>
    private bool Matches(Account a, string text)
    {
        bool Has(string value) => value.Contains(text, StringComparison.OrdinalIgnoreCase);

        return SearchField switch
        {
            AccountSearchField.FirstName => Has(a.Billing.FirstName) || Has(a.Service.FirstName) || Has(a.SecondServiceName),
            AccountSearchField.AccountNumber => Has(a.AccountNumber),
            AccountSearchField.StreetAddress => Has(a.Billing.StreetLine) || Has(a.Service.StreetLine),
            _ => Has(a.Billing.LastName) || Has(a.Service.LastName) || Has(a.Billing.Company),
        };
    }
}
