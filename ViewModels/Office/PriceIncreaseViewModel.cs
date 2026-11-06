using System.Collections.ObjectModel;
using BillyGoat_UI.Models;
using BillyGoat_UI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BillyGoat_UI.ViewModels;

/// <summary>One row of the price increase preview: a service with its old and new price.</summary>
public class PriceChangeLine
{
    public required Account Account { get; init; }
    public required ServiceRecord Service { get; init; }
    public required decimal NewPrice { get; init; }

    public string AccountText => $"{Account.AccountNumber}  {Account.BillingName}";
}

/// <summary>
/// Office tool 4: Service Price Increase. Raises the price of open services that match
/// the choices. Accounts with "Hold current prices" (Accounting Defaults) are skipped.
/// </summary>
public partial class PriceIncreaseViewModel : ObservableObject
{
    private readonly Action _close;

    public PriceIncreaseViewModel(Action close)
    {
        _close = close;
        Services = Lookups.Services.Select(s => new CheckItem(s)).ToList();
        FromDate = FakeData.Today;
        ToDate = new DateTime(FakeData.Today.Year + 1, 12, 31);
        PriceMin = "";
        PriceMax = "";
        PriceStartFrom = new DateTime(FakeData.Today.Year - 1, 1, 1);
        PriceStartTo = FakeData.Today;
        IncreaseText = "5.00";
        IsPercent = false;
        StatusMessage = "";
    }

    /// <summary>The services checklist.</summary>
    public List<CheckItem> Services { get; }

    /// <summary>Services scheduled between these dates.</summary>
    [ObservableProperty] public partial DateTime? FromDate { get; set; }
    [ObservableProperty] public partial DateTime? ToDate { get; set; }

    /// <summary>Only prices in this range are adjusted (empty = no limit).</summary>
    [ObservableProperty] public partial string PriceMin { get; set; }
    [ObservableProperty] public partial string PriceMax { get; set; }

    /// <summary>Only prices that started (were sold) between these dates.</summary>
    [ObservableProperty] public partial DateTime? PriceStartFrom { get; set; }
    [ObservableProperty] public partial DateTime? PriceStartTo { get; set; }

    [ObservableProperty] public partial string IncreaseText { get; set; }

    /// <summary>false = increase by a dollar amount, true = by a percent.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDollars))]
    public partial bool IsPercent { get; set; }

    public bool IsDollars
    {
        get => !IsPercent;
        set => IsPercent = !value;
    }

    /// <summary>The services that will change (filled by Preview).</summary>
    public ObservableCollection<PriceChangeLine> Matches { get; } = new();

    [ObservableProperty] public partial string StatusMessage { get; set; }

    [RelayCommand]
    private void CheckAll() => Services.ForEach(s => s.IsChecked = true);

    [RelayCommand]
    private void UncheckAll() => Services.ForEach(s => s.IsChecked = false);

    /// <summary>Preview: lists the services that would change, with old and new prices.</summary>
    [RelayCommand]
    private void Preview()
    {
        if (TryGetIncrease(out decimal increase))
            FindMatches(increase);
    }

    /// <summary>Apply: asks for confirmation, then changes the prices.</summary>
    [RelayCommand]
    private void Apply()
    {
        if (!TryGetIncrease(out decimal increase))
            return;

        FindMatches(increase);
        if (Matches.Count == 0)
        {
            MessageService.Show("No open services match these choices, so no prices were changed.");
            return;
        }

        string how = IsPercent ? $"{increase}%" : Formats.Money(increase);
        if (!MessageService.Confirm($"Increase the price of {Matches.Count} service(s) by {how}?\n\nThis cannot be undone."))
            return;

        foreach (PriceChangeLine line in Matches)
            line.Service.SaleAmount = line.NewPrice;

        StatusMessage = $"{Matches.Count} price(s) increased by {how}.";
        MessageService.Show(StatusMessage);
        Matches.Clear();
    }

    private bool TryGetIncrease(out decimal increase)
    {
        if (!decimal.TryParse(IncreaseText, System.Globalization.NumberStyles.Currency, Formats.Us, out increase) || increase <= 0)
        {
            MessageService.Warn("Increase amount must be a number greater than 0, for example 5.00.");
            return false;
        }
        return true;
    }

    private void FindMatches(decimal increase)
    {
        var services = Services.Where(s => s.IsChecked).Select(s => s.Name).ToList();
        decimal min = decimal.TryParse(PriceMin, out decimal a) ? a : decimal.MinValue;
        decimal max = decimal.TryParse(PriceMax, out decimal b) ? b : decimal.MaxValue;

        Matches.Clear();
        foreach (Account account in FakeData.Accounts.Where(a => !a.HoldCurrentPrices))
        {
            foreach (ServiceRecord s in account.Services.Where(s => s.IsOutstanding))
            {
                if (!services.Any(name => s.ServiceName.StartsWith(name, StringComparison.OrdinalIgnoreCase))) continue;
                if (FromDate.HasValue && s.DateScheduled < FromDate.Value) continue;
                if (ToDate.HasValue && s.DateScheduled > ToDate.Value) continue;
                if (s.SaleAmount < min || s.SaleAmount > max) continue;
                if (PriceStartFrom.HasValue && s.DateSold < PriceStartFrom.Value) continue;
                if (PriceStartTo.HasValue && s.DateSold > PriceStartTo.Value) continue;

                decimal newPrice = IsPercent
                    ? Math.Round(s.SaleAmount * (1 + increase / 100m), 2)
                    : s.SaleAmount + increase;
                Matches.Add(new PriceChangeLine { Account = account, Service = s, NewPrice = newPrice });
            }
        }
        StatusMessage = Matches.Count == 0 ? "No open services match these choices." : $"{Matches.Count} service(s) will change.";
    }

    [RelayCommand]
    private void Close() => _close();
}
