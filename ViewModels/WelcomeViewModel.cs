using System.Collections.ObjectModel;
using System.Globalization;
using BillyGoat_UI.Models;
using BillyGoat_UI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BillyGoat_UI.ViewModels;

/// <summary>
/// ViewModel for the Welcome (home) page: today's stats, the customer lookup,
/// prepayment letters and the report links.
/// </summary>
public partial class WelcomeViewModel : ObservableObject
{
    // US culture so money always shows as "$1,860" no matter the PC's region settings.
    private static readonly CultureInfo Us = CultureInfo.GetCultureInfo("en-US");

    // Every customer. The list on screen (Customers) is a filtered copy of this.
    private readonly List<Customer> _allCustomers = FakeData.GetCustomers();

    public WelcomeViewModel()
    {
        SearchText = "";
        SearchField = CustomerSearchField.LastName;
        StatusFilter = CustomerStatusFilter.Active;
        PrepaymentCustomer = "Whitfield, Rachel (C-01842)";
        ApplyFilter();
    }

    // ===== Header and stat boxes =====

    /// <summary>"Tuesday, October 6 · 12 of 31 stops done today"</summary>
    public string Subtitle =>
        $"{FakeData.Today.ToString("dddd, MMMM d", Us)} · {FakeData.StopsDoneToday} of {FakeData.StopsScheduledToday} stops done today";

    public string EarnedToday => FakeData.EarnedToday.ToString("C0", Us);
    public string PastDue => FakeData.PastDue.ToString("C0", Us);
    public string ApplicationsDueThisWeek => FakeData.ApplicationsDueThisWeek.ToString(Us);

    // ===== 1. Customer lookup =====

    /// <summary>The customers currently shown in the list.</summary>
    public ObservableCollection<Customer> Customers { get; } = new();

    /// <summary>Which field to search (bound to the "Search by" radio buttons).</summary>
    [ObservableProperty]
    public partial CustomerSearchField SearchField { get; set; }

    /// <summary>Text typed in the search box.</summary>
    [ObservableProperty]
    public partial string SearchText { get; set; }

    /// <summary>Active / All / Inactive (bound to the radio row under the list).</summary>
    [ObservableProperty]
    public partial CustomerStatusFilter StatusFilter { get; set; }

    /// <summary>The highlighted row in the list.</summary>
    [ObservableProperty]
    public partial Customer? SelectedCustomer { get; set; }

    /// <summary>Grey hint text shown in the empty search box, e.g. "Type a last name".</summary>
    public string SearchHint => SearchField switch
    {
        CustomerSearchField.FirstName => "Type a first name",
        CustomerSearchField.CustomerNumber => "Type a customer number",
        CustomerSearchField.StreetAddress => "Type a street address",
        _ => "Type a last name",
    };

    // When the search field changes, the hint text must change too.
    partial void OnSearchFieldChanged(CustomerSearchField value) => OnPropertyChanged(nameof(SearchHint));

    // Changing Active/All/Inactive filters the list right away.
    partial void OnStatusFilterChanged(CustomerStatusFilter value) => ApplyFilter();

    /// <summary>Search button (or Enter in the search box).</summary>
    [RelayCommand]
    private void Search() => ApplyFilter();

    /// <summary>Double-click or Enter on a customer row.</summary>
    [RelayCommand]
    private void OpenCustomer(Customer? customer)
    {
        if (customer == null) return;
        MessageService.Show("Customer profile coming soon");
    }

    /// <summary>
    /// Rebuilds the visible list from all customers, using the status filter
    /// and the search text in the chosen field (case-insensitive "contains").
    /// </summary>
    private void ApplyFilter()
    {
        string text = (SearchText ?? "").Trim();

        IEnumerable<Customer> results = _allCustomers;

        // 1. Active / All / Inactive
        if (StatusFilter == CustomerStatusFilter.Active)
            results = results.Where(c => c.IsActive);
        else if (StatusFilter == CustomerStatusFilter.Inactive)
            results = results.Where(c => !c.IsActive);

        // 2. Search text in the chosen field (empty text = no text filter)
        if (text.Length > 0)
            results = results.Where(c => GetSearchValue(c).Contains(text, StringComparison.OrdinalIgnoreCase));

        // Keep the same customer selected if they are still in the results.
        Customer? previous = SelectedCustomer;

        Customers.Clear();
        foreach (Customer c in results)
            Customers.Add(c);

        SelectedCustomer = previous != null && Customers.Contains(previous)
            ? previous
            : Customers.FirstOrDefault();
    }

    /// <summary>Returns the value of the field the user chose to search by.</summary>
    private string GetSearchValue(Customer c) => SearchField switch
    {
        CustomerSearchField.FirstName => c.FirstName,
        CustomerSearchField.CustomerNumber => c.CustomerNumber,
        CustomerSearchField.StreetAddress => c.StreetAddress,
        _ => c.LastName,
    };

    // ===== 2. Create prepayment letters =====

    [ObservableProperty]
    public partial string PrepaymentCustomer { get; set; }

    [RelayCommand]
    private void PrintLetter() => MessageService.Show($"Printing prepayment letter for {PrepaymentCustomer}.\n\n(Printing is coming soon.)");

    [RelayCommand]
    private void EmailPdf() => MessageService.Show($"Emailing prepayment letter PDF to {PrepaymentCustomer}.\n\n(Email is coming soon.)");

    // ===== 3. Reporting system =====

    /// <summary>Every report link uses this command; the report name is the CommandParameter.</summary>
    [RelayCommand]
    private void OpenReport(string reportName) => MessageService.Show($"{reportName}\n\nThis report is coming soon.");
}
