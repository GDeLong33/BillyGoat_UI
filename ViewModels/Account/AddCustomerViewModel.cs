using BillyGoat_UI.Models;
using BillyGoat_UI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BillyGoat_UI.ViewModels;

/// <summary>Where the bill goes for a new customer (the large radio buttons).</summary>
public enum BillingChoice
{
    SameAsService,
    MainAccount,
    CorporateAccount,
    OtherAddress
}

/// <summary>
/// Add New Customer dialog. Step 1: service address and billing choice.
/// Step 2 (after OK): the Required Fields pop-up. When both are done the new
/// account is created; MainViewModel then opens it on Contact Information.
/// </summary>
public partial class AddCustomerViewModel : DialogViewModel
{
    public AddCustomerViewModel()
    {
        BillingChoice = BillingChoice.SameAsService;
        ErrorMessage = "";
        ParentAccounts = FakeData.Accounts.Where(a => a.IsActive).OrderBy(a => a.BillingName).ToList();
    }

    public override string Title => "Add New Customer";

    /// <summary>The service address being typed (shown with the shared AddressFields view).</summary>
    public Address ServiceAddress { get; } = new();

    /// <summary>Only used when "Bill to another address" is chosen.</summary>
    public Address OtherBillingAddress { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowParentAccount))]
    [NotifyPropertyChangedFor(nameof(ShowOtherAddress))]
    [NotifyPropertyChangedFor(nameof(ParentAccountLabel))]
    public partial BillingChoice BillingChoice { get; set; }

    /// <summary>Accounts that can be the main or corporate account.</summary>
    public List<Account> ParentAccounts { get; }

    [ObservableProperty]
    public partial Account? ParentAccount { get; set; }

    public bool ShowParentAccount => BillingChoice is BillingChoice.MainAccount or BillingChoice.CorporateAccount;
    public bool ShowOtherAddress => BillingChoice == BillingChoice.OtherAddress;
    public string ParentAccountLabel => BillingChoice == BillingChoice.CorporateAccount ? "Corporate account" : "Main account";

    [ObservableProperty]
    public partial string ErrorMessage { get; set; }

    /// <summary>The account that was created (set when the dialog closes with OK).</summary>
    public Account? CreatedAccount { get; private set; }

    [RelayCommand]
    private void Ok()
    {
        ErrorMessage = CheckInput();
        if (ErrorMessage.Length > 0)
            return;

        // Step 2: some fields are required before the account can be saved.
        RequiredFieldsViewModel required = new();
        if (!DialogService.ShowDialog(required))
            return;   // back to this dialog, nothing is lost

        CreatedAccount = CreateAccount(required);
        Close(true);
    }

    [RelayCommand]
    private void Cancel() => Close(false);

    /// <summary>Returns an error message, or "" when everything needed is filled in.</summary>
    private string CheckInput()
    {
        Address a = ServiceAddress;
        if (a.LastName.Trim().Length == 0 && a.Company.Trim().Length == 0)
            return "Enter a last name or a company name.";
        if (a.StreetName.Trim().Length == 0 || a.City.Trim().Length == 0 || a.Zip.Trim().Length == 0)
            return "Enter the street name, city and zip of the service address.";
        if (ShowParentAccount && ParentAccount == null)
            return $"Choose the {ParentAccountLabel.ToLowerInvariant()} to bill.";
        if (ShowOtherAddress && (OtherBillingAddress.StreetName.Trim().Length == 0 || OtherBillingAddress.City.Trim().Length == 0))
            return "Enter the billing street name and city.";
        return "";
    }

    private Account CreateAccount(RequiredFieldsViewModel required)
    {
        Account account = new()
        {
            AccountNumber = FakeData.NewAccountNumber(),
            StartDate = FakeData.Today,
            StatusDate = FakeData.Today,
            PropertyType = required.PropertyType!,
            Route = required.Route!,
            Salesperson = required.Salesperson!,
            Sort = "D - Any time",
            Frequency = "Every 6 weeks",
            PrenotifyType = "None",
            CreatedDate = FakeData.Today,
            CreatedBy = Session.UserName,
            EditedDate = FakeData.Today,
            EditedBy = Session.UserName,
        };
        account.Status = required.Status!;
        account.Service.CopyFrom(ServiceAddress);

        switch (BillingChoice)
        {
            case BillingChoice.SameAsService:
                account.Billing.CopyFrom(ServiceAddress);
                break;
            case BillingChoice.MainAccount:
            case BillingChoice.CorporateAccount:
                // The bill goes to the other account's billing address.
                account.Billing.CopyFrom(ParentAccount!.Billing);
                account.BillingType = "Sub";
                account.Notes.Add(new OfficeNote
                {
                    Timestamp = FakeData.Now, EnteredBy = Session.UserName,
                    Text = $"Bills to {ParentAccountLabel.ToLowerInvariant()} {ParentAccount.AccountNumber} ({ParentAccount.BillingName}).",
                });
                break;
            case BillingChoice.OtherAddress:
                account.Billing.CopyFrom(OtherBillingAddress);
                break;
        }

        FakeData.Accounts.Add(account);
        return account;
    }
}

/// <summary>
/// The pop-up after Add New Customer: "These fields are required".
/// Finish only works when all four are chosen.
/// </summary>
public partial class RequiredFieldsViewModel : DialogViewModel
{
    public RequiredFieldsViewModel()
    {
        ErrorMessage = "";
        Status = "Active";
    }

    public override string Title => "Required Fields";

    public string[] Statuses => Lookups.AccountStatuses;

    [ObservableProperty] public partial string? PropertyType { get; set; }
    [ObservableProperty] public partial string? Route { get; set; }
    [ObservableProperty] public partial string? Status { get; set; }
    [ObservableProperty] public partial string? Salesperson { get; set; }
    [ObservableProperty] public partial string ErrorMessage { get; set; }

    [RelayCommand]
    private void Finish()
    {
        var missing = new List<string>();
        if (string.IsNullOrEmpty(PropertyType)) missing.Add("Property type");
        if (string.IsNullOrEmpty(Route)) missing.Add("Route");
        if (string.IsNullOrEmpty(Status)) missing.Add("Status");
        if (string.IsNullOrEmpty(Salesperson)) missing.Add("Salesperson");

        if (missing.Count > 0)
        {
            ErrorMessage = "Please choose: " + string.Join(", ", missing) + ".";
            return;
        }
        Close(true);
    }

    [RelayCommand]
    private void Cancel() => Close(false);
}
