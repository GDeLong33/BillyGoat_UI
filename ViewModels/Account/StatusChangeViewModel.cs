using BillyGoat_UI.Models;
using BillyGoat_UI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BillyGoat_UI.ViewModels;

/// <summary>
/// Status Change dialog. Shows what the change will affect (open services,
/// payment plans, the balance...) and asks why. Nothing changes until Finish.
/// </summary>
public partial class StatusChangeViewModel : DialogViewModel
{
    private readonly Account _account;

    public StatusChangeViewModel(Account account, string newStatus)
    {
        _account = account;
        PreviousStatus = account.Status;
        NewStatus = newStatus;
        Reason = "";
        ErrorMessage = "";

        OutstandingServices = account.Services.Where(s => s.IsOutstanding).ToList();
        PrintedServices = account.Services.Where(s => s.Status == ServiceRecord.Printed).ToList();

        // When re-activating, the inactive services are listed with a checkbox each
        // (checked = make this service active again).
        InactiveServices = account.Services
            .Where(s => s.Status == ServiceRecord.Inactive)
            .Select(s => new CheckItem($"{s.ServiceName}  ({Formats.Date(s.DateScheduled)})", isChecked: IsBecomingActive, tag: s))
            .ToList();
    }

    public override string Title => "Status Change";

    public string AccountText => $"{_account.AccountNumber}  {_account.BillingName}";
    public string PreviousStatus { get; }
    public string NewStatus { get; }

    private bool IsBecomingActive => NewStatus == "Active";

    /// <summary>"Why are you changing status" (required).</summary>
    [ObservableProperty]
    public partial string Reason { get; set; }

    [ObservableProperty]
    public partial string ErrorMessage { get; set; }

    /// <summary>The information box: explains what Finish will do.</summary>
    public string InformationText => NewStatus switch
    {
        "Active" => "The account will be active again. Services you check under \"Services marked inactive\" will become open again and can be printed on work orders.",
        "On hold" => "Work orders will not print for this account while it is on hold. Outstanding services are marked inactive.",
        "Prospect" => "The account becomes a prospect (not a customer yet). Outstanding services are marked inactive.",
        _ => $"The account will be {NewStatus.ToLowerInvariant()}. Outstanding services below are marked inactive and will not print on work orders. The balance is still owed.",
    };

    public List<ServiceRecord> OutstandingServices { get; }
    public List<ServiceRecord> PrintedServices { get; }
    public List<CheckItem> InactiveServices { get; }

    public bool HasOutstandingServices => OutstandingServices.Count > 0;
    public bool HasPrintedServices => PrintedServices.Count > 0;
    public bool ShowInactiveServices => IsBecomingActive;

    /// <summary>Special payment plans on this account (from Accounting Defaults).</summary>
    public string PaymentPlanText => _account.PayType == "Normal"
        ? "No special payment plan."
        : $"This account is on a special payment plan: {_account.PayType}. Check it before changing status.";

    public string BalanceText => Formats.Money(_account.Balance);

    /// <summary>Print button next to the outstanding balance: shows a statement.</summary>
    [RelayCommand]
    private void PrintStatement() => PrintService.ShowPreview(ReportFactory.Statement(_account), "Statement");

    [RelayCommand]
    private void Finish()
    {
        if (Reason.Trim().Length == 0)
        {
            ErrorMessage = "Please say why you are changing the status.";
            return;
        }

        _account.Status = NewStatus;
        _account.StatusDate = FakeData.Today;
        _account.StatusDescription = Reason.Trim();
        _account.EditedDate = FakeData.Today;
        _account.EditedBy = Session.UserName;

        if (IsBecomingActive)
        {
            foreach (CheckItem item in InactiveServices.Where(i => i.IsChecked))
                ((ServiceRecord)item.Tag!).Status = ServiceRecord.Open;
        }
        else
        {
            foreach (ServiceRecord service in OutstandingServices)
                service.Status = ServiceRecord.Inactive;
        }

        // Keep a record of the change in the office notes.
        _account.Notes.Insert(0, new OfficeNote
        {
            Timestamp = FakeData.Now,
            EnteredBy = Session.UserName,
            Text = $"Status changed from {PreviousStatus} to {NewStatus}: {Reason.Trim()}",
        });

        Close(true);
    }

    [RelayCommand]
    private void Cancel() => Close(false);
}
