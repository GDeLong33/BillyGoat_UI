using BillyGoat_UI.Models;
using BillyGoat_UI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BillyGoat_UI.ViewModels;

/// <summary>
/// Office tool 5: Print Prepay / Renewal Letters. Finds active accounts with the
/// checked services in the date range and prints one letter each. After printing,
/// "mark as sent" fills in the account's Marketing Letters screen automatically.
/// </summary>
public partial class PrepayLettersViewModel : ObservableObject
{
    private readonly Action _close;

    public PrepayLettersViewModel(Action close)
    {
        _close = close;
        FromDate = new DateTime(FakeData.Today.Year, 1, 1);
        ToDate = new DateTime(FakeData.Today.Year, 12, 31);
        Services = Lookups.Services.Select(s => new CheckItem(s, isChecked: s is "Fertilization" or "Grub control" or "Mowing")).ToList();
        LetterSize = Lookups.LetterSizes[0];
        LetterType = Lookups.LetterTypes[0];
        PrepayPercentText = "5";
        ExpirationDate = new DateTime(FakeData.Today.Year + 1, 3, 1);
        PrintBillingOnStub = true;
        StatusMessage = "";
    }

    [ObservableProperty] public partial DateTime? FromDate { get; set; }
    [ObservableProperty] public partial DateTime? ToDate { get; set; }
    public List<CheckItem> Services { get; }
    public string[] LetterSizes => Lookups.LetterSizes;
    public string[] LetterTypes => Lookups.LetterTypes;
    [ObservableProperty] public partial string LetterSize { get; set; }
    [ObservableProperty] public partial string LetterType { get; set; }
    [ObservableProperty] public partial string PrepayPercentText { get; set; }
    [ObservableProperty] public partial DateTime? ExpirationDate { get; set; }
    [ObservableProperty] public partial bool PrintBillingOnStub { get; set; }
    [ObservableProperty] public partial string StatusMessage { get; set; }

    [RelayCommand]
    private void CheckAll() => Services.ForEach(s => s.IsChecked = true);

    [RelayCommand]
    private void UncheckAll() => Services.ForEach(s => s.IsChecked = false);

    /// <summary>Preview: just shows the letters.</summary>
    [RelayCommand]
    private void Preview()
    {
        var letters = FindLetters();
        if (letters == null) return;
        PrintService.ShowPreview(ReportFactory.Letters(letters, LetterType, LetterSize, Percent, ExpirationDate, PrintBillingOnStub), "Letters");
    }

    /// <summary>Print: shows the preview (with its Print button), then asks to mark the letters as sent.</summary>
    [RelayCommand]
    private void Print()
    {
        var letters = FindLetters();
        if (letters == null) return;

        var document = ReportFactory.Letters(letters, LetterType, LetterSize, Percent, ExpirationDate, PrintBillingOnStub);
        if (!PrintService.PreviewThenAsk(document, "Letters", $"Mark these {letters.Count} letter(s) as printed and sent?"))
            return;

        // Fill in each account's Marketing Letters screen.
        foreach ((Account account, List<ServiceRecord> services) in letters)
        {
            decimal total = services.Sum(s => s.SaleAmount);
            account.Letters.Add(new LetterRecord
            {
                DateSent = FakeData.Today,
                LetterType = LetterType,
                Services = string.Join(", ", services.Select(s => s.ServiceName).Distinct()),
                PrepayPercent = Percent,
                PrepayAmount = Math.Round(total * (1 - Percent / 100m), 2),
                ExpirationDate = ExpirationDate,
            });
            account.PrepayPercent = Percent;
            account.RenewalLetter = $"{LetterType} letter sent {Formats.Date(FakeData.Today)}";
        }
        StatusMessage = $"{letters.Count} letter(s) marked as sent. See each account's Marketing Letters screen.";
    }

    [RelayCommand]
    private void CustomizeForm() =>
        MessageService.Show("Customize form would open the letter editor (wording, logo and layout).\n\nThe letter editor is not part of this demo.");

    private decimal Percent => decimal.TryParse(PrepayPercentText, out decimal p) ? p : 0;

    /// <summary>Active accounts with checked services scheduled in the date range. Null (and a message) if none.</summary>
    private List<(Account Account, List<ServiceRecord> Services)>? FindLetters()
    {
        var names = Services.Where(s => s.IsChecked).Select(s => s.Name).ToList();
        var letters = new List<(Account, List<ServiceRecord>)>();

        foreach (Account account in FakeData.Accounts.Where(a => a.IsActive))
        {
            var services = account.Services.Where(s =>
                s.Status != ServiceRecord.Inactive &&
                names.Any(n => s.ServiceName.StartsWith(n, StringComparison.OrdinalIgnoreCase)) &&
                (!FromDate.HasValue || s.DateScheduled >= FromDate.Value) &&
                (!ToDate.HasValue || s.DateScheduled <= ToDate.Value)).ToList();

            if (services.Count > 0)
                letters.Add((account, services));
        }

        if (letters.Count == 0)
        {
            MessageService.Show("No active accounts have the checked services in this date range.");
            return null;
        }
        return letters;
    }

    [RelayCommand]
    private void Close() => _close();
}
