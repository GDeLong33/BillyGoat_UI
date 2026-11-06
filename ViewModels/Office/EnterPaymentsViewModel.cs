using System.Collections.ObjectModel;
using System.ComponentModel;
using BillyGoat_UI.Models;
using BillyGoat_UI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BillyGoat_UI.ViewModels;

/// <summary>How a payment for work not done yet is recorded.</summary>
public enum AdvancePaymentOption { Prepayment, Deposit, Percent }

/// <summary>Where money left over after paying the checked services goes.</summary>
public enum RemainingMoneyOption { ApplyToService, CashOnAccount, FinanceChargePaid }

/// <summary>One row of the payment listing table: a service with a "Pay" checkbox.</summary>
public partial class PaymentLine : ObservableObject
{
    public PaymentLine(ServiceRecord service) => Service = service;

    public ServiceRecord Service { get; }

    /// <summary>The "Pay" checkbox.</summary>
    [ObservableProperty]
    public partial bool IsPaying { get; set; }
}

/// <summary>
/// Office tool 2: Enter Payments. Also opened from an account (Accounting > Enter a payment);
/// it is the same ViewModel and view, so it works the same in both places.
///
/// The form fills in as you go: the listing appears after a valid account number,
/// "where to apply remaining money" after an amount, and Accept once everything needed is there.
/// </summary>
public partial class EnterPaymentsViewModel : ObservableObject
{
    private readonly Action _close;

    public EnterPaymentsViewModel(Batch batch, Action close, Account? startAccount)
    {
        Batch = batch;
        _close = close;
        PaymentDate = batch.Date;
        PaymentType = Lookups.PaymentTypes[0];
        AmountText = "";
        CheckNumber = "";
        AdvanceOption = AdvancePaymentOption.Prepayment;
        RemainingOption = RemainingMoneyOption.CashOnAccount;
        StatusMessage = "";
        AccountNumberText = startAccount?.AccountNumber ?? "";
    }

    public Batch Batch { get; }
    public string[] PaymentTypes => Lookups.PaymentTypes;

    // ===== Account information =====

    /// <summary>Account number or a barcode (invoice number) from the bill stub.</summary>
    [ObservableProperty]
    public partial string AccountNumberText { get; set; }

    /// <summary>The account found from AccountNumberText, or null.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasAccount))]
    [NotifyPropertyChangedFor(nameof(AccountHeaderText))]
    public partial Account? Account { get; set; }

    public bool HasAccount => Account != null;

    /// <summary>"Batch R-1043   A-01842 Whitfield, Rachel   340 Cedar Ct, Saginaw, MI 48603"</summary>
    public string AccountHeaderText => Account == null
        ? (AccountNumberText.Trim().Length >= 5 ? "No account found. Type an account number such as A-01842, or scan a bill barcode." : "")
        : $"Batch {Batch.Number}     {Account.AccountNumber}  {Account.BillingName}     {Account.BillingAddressText}";

    partial void OnAccountNumberTextChanged(string value)
    {
        // An account number, or an invoice number from a bill stub barcode.
        Account = FakeData.FindAccount(value) ?? FakeData.FindInvoice(value)?.Account;
        OnPropertyChanged(nameof(AccountHeaderText));
    }

    partial void OnAccountChanged(Account? value) => FillListing();

    // ===== Payment details =====

    [ObservableProperty] public partial DateTime PaymentDate { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCheck))]
    public partial string PaymentType { get; set; }

    public bool IsCheck => PaymentType == "Check";

    [ObservableProperty] public partial string AmountText { get; set; }
    [ObservableProperty] public partial string CheckNumber { get; set; }
    [ObservableProperty] public partial AdvancePaymentOption AdvanceOption { get; set; }
    [ObservableProperty] public partial RemainingMoneyOption RemainingOption { get; set; }

    /// <summary>The amount as a number, or 0 if it is not a valid number yet.</summary>
    private decimal Amount =>
        decimal.TryParse(AmountText, System.Globalization.NumberStyles.Currency, Formats.Us, out decimal a) && a > 0 ? a : 0;

    public bool HasAmount => Amount > 0;

    // ===== Listing =====

    public ObservableCollection<PaymentLine> Lines { get; } = new();

    /// <summary>The highlighted row in the listing.</summary>
    [ObservableProperty] public partial PaymentLine? SelectedLine { get; set; }

    /// <summary>Space bar on a row: checks or unchecks its Pay box (keyboard users).</summary>
    [RelayCommand]
    private void TogglePay()
    {
        if (SelectedLine != null)
            SelectedLine.IsPaying = !SelectedLine.IsPaying;
    }

    /// <summary>Total balance of the checked rows.</summary>
    private decimal CheckedTotal => Lines.Where(l => l.IsPaying).Sum(l => Math.Max(0, l.Service.Balance));

    private decimal Remaining => Math.Max(0, Amount - CheckedTotal);

    public string CheckedTotalText => $"Checked: {Formats.Money(CheckedTotal)}";
    public string RemainingText => $"Remaining money: {Formats.Money(Remaining)}";

    /// <summary>
    /// The Accept button appears when the form is complete: account found, amount
    /// entered, a check number for checks, and at least one row checked.
    /// </summary>
    public bool CanAccept =>
        Account != null && Amount > 0 && Lines.Any(l => l.IsPaying) && (!IsCheck || CheckNumber.Trim().Length > 0);

    [ObservableProperty] public partial string StatusMessage { get; set; }

    /// <summary>Payments accepted in this batch (shown at the bottom).</summary>
    public ObservableCollection<PostedLine> AcceptedPayments { get; } = new();

    /// <summary>Fills the listing with the account's services that still have something owing.</summary>
    private void FillListing()
    {
        foreach (PaymentLine line in Lines)
            line.PropertyChanged -= Line_PropertyChanged;
        Lines.Clear();

        if (Account != null)
        {
            foreach (ServiceRecord service in Account.Services
                         .Where(s => s.Status != ServiceRecord.Inactive && s.Balance > 0)
                         .OrderBy(s => s.DateScheduled))
            {
                PaymentLine line = new(service);
                line.PropertyChanged += Line_PropertyChanged;
                Lines.Add(line);
            }
        }
        RefreshTotals();
    }

    // When a Pay checkbox changes, the totals and the Accept button must update.
    private void Line_PropertyChanged(object? sender, PropertyChangedEventArgs e) => RefreshTotals();

    // Any change to these fields can change the totals or whether Accept is shown.
    partial void OnAmountTextChanged(string value) => RefreshTotals();
    partial void OnCheckNumberChanged(string value) => RefreshTotals();
    partial void OnPaymentTypeChanged(string value) => RefreshTotals();

    private void RefreshTotals()
    {
        OnPropertyChanged(nameof(HasAmount));
        OnPropertyChanged(nameof(CheckedTotalText));
        OnPropertyChanged(nameof(RemainingText));
        OnPropertyChanged(nameof(CanAccept));
    }

    /// <summary>
    /// Accept: pays the checked services in order (oldest first), then puts any
    /// money left over where the user chose.
    /// </summary>
    [RelayCommand]
    private void Accept()
    {
        if (!CanAccept || Account == null) return;

        decimal left = Amount;
        string type = IsCheck ? $"Check #{CheckNumber.Trim()}" : PaymentType;

        foreach (PaymentLine line in Lines.Where(l => l.IsPaying))
        {
            ServiceRecord service = line.Service;
            decimal pay = Math.Min(left, service.Balance);
            if (pay <= 0) continue;

            service.Payments += pay;
            left -= pay;

            // Paying for work that is not done yet is an advance payment.
            string memo = service.Status == ServiceRecord.Completed
                ? $"{service.ServiceName} (invoice {service.InvoiceNumber})"
                : $"{AdvanceOption} for {service.ServiceName} (invoice {service.InvoiceNumber})";
            Account.Adjustments.Add(new AdjustmentEntry
            {
                EntryDate = PaymentDate, Transaction = "Payment", Type = type, Memo = memo,
                Total = -pay, Batch = Batch.Number, Service = service,
            });
        }

        if (left > 0)
        {
            ApplyRemaining(left, type);
        }

        AcceptedPayments.Add(new PostedLine
        {
            InvoiceNumber = type, AccountText = $"{Account.AccountNumber} {Account.BillingName}",
            ServiceName = string.Join(", ", Lines.Where(l => l.IsPaying).Select(l => l.Service.ServiceName)),
            Employee = Session.UserName, Amount = Amount,
        });
        StatusMessage = $"Payment of {Formats.Money(Amount)} accepted for {Account.BillingName} (batch {Batch.Number}).";

        // Ready for the next payment.
        AmountText = "";
        CheckNumber = "";
        FillListing();
    }

    /// <summary>Puts leftover money where "Select where to apply remaining money" says.</summary>
    private void ApplyRemaining(decimal left, string type)
    {
        switch (RemainingOption)
        {
            case RemainingMoneyOption.ApplyToService:
                // Overpay the last checked service; it then shows a credit balance.
                ServiceRecord last = Lines.Last(l => l.IsPaying).Service;
                last.Payments += left;
                Account!.Adjustments.Add(new AdjustmentEntry
                {
                    EntryDate = PaymentDate, Transaction = "Payment", Type = type, Batch = Batch.Number, Service = last,
                    Memo = $"Extra applied to {last.ServiceName}", Total = -left,
                });
                break;
            case RemainingMoneyOption.CashOnAccount:
                Account!.Adjustments.Add(new AdjustmentEntry
                {
                    EntryDate = PaymentDate, Transaction = "Cash on account", Type = type, Batch = Batch.Number,
                    Memo = "Money left over, kept as a credit on the account", Total = -left,
                });
                break;
            case RemainingMoneyOption.FinanceChargePaid:
                Account!.Adjustments.Add(new AdjustmentEntry
                {
                    EntryDate = PaymentDate, Transaction = "Finance charge paid", Type = type, Batch = Batch.Number,
                    Memo = "Money left over, applied to finance charges", Total = -left,
                });
                break;
        }
    }

    [RelayCommand]
    private void Close() => _close();
}
