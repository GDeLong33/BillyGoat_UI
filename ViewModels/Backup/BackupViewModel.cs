using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text;
using BillyGoat_UI.Models;
using BillyGoat_UI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;

namespace BillyGoat_UI.ViewModels;

/// <summary>
/// Backup &amp; Export tab (administrators only).
/// Left: back up the database to a folder, and see past backups (restoring is
/// simulated, since the demo only has sample data). Right: export accounts,
/// services or employees to CSV files that open in Excel.
/// </summary>
public partial class BackupViewModel : ObservableObject
{
    public BackupViewModel()
    {
        BackupFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Billy Goat Backups");
        StatusMessage = "";
        ExportItems =
        [
            new CheckItem(Accounts, isChecked: true),
            new CheckItem(Services, isChecked: true),
            new CheckItem(Employees),
        ];

        // A few sample backups so the history list is not empty.
        History.Add(new BackupRecord(FakeData.Today.AddDays(-1).AddHours(18), "Gavin DeLong", BackupFolder, "Automatic"));
        History.Add(new BackupRecord(FakeData.Today.AddDays(-8).AddHours(18), "Gavin DeLong", BackupFolder, "Automatic"));
        History.Add(new BackupRecord(FakeData.Today.AddDays(-12).AddHours(9.5), "Gavin DeLong", BackupFolder, "Manual"));
        SelectedBackup = History[0];
    }

    // ===== Backup =====

    /// <summary>Where "Back up now" saves to.</summary>
    [ObservableProperty]
    public partial string BackupFolder { get; set; }

    /// <summary>Past backups, newest first.</summary>
    public ObservableCollection<BackupRecord> History { get; } = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RestoreCommand))]
    public partial BackupRecord? SelectedBackup { get; set; }

    /// <summary>Green text under the buttons, e.g. "Backup finished."</summary>
    [ObservableProperty]
    public partial string StatusMessage { get; set; }

    [RelayCommand]
    private void BrowseFolder()
    {
        OpenFolderDialog dialog = new() { Title = "Choose the backup folder" };
        if (Directory.Exists(BackupFolder))
            dialog.InitialDirectory = BackupFolder;
        if (dialog.ShowDialog() == true)
            BackupFolder = dialog.FolderName;
    }

    [RelayCommand]
    private void BackUpNow()
    {
        if (string.IsNullOrWhiteSpace(BackupFolder))
        {
            MessageService.Warn("Choose a backup folder first.");
            return;
        }

        BackupRecord record = new(FakeData.Now, Session.UserName, BackupFolder, "Manual");
        History.Insert(0, record);
        SelectedBackup = record;
        StatusMessage = $"Backup finished at {record.DateText}.";
    }

    private bool CanRestore() => SelectedBackup != null;

    [RelayCommand(CanExecute = nameof(CanRestore))]
    private void Restore()
    {
        BackupRecord backup = SelectedBackup!;
        if (!MessageService.Confirm($"Restore the backup from {backup.DateText}?\n\n" +
                                    "Everything entered since then will be replaced. Other users should sign out first."))
            return;

        // Demo only: the sample data is not changed.
        StatusMessage = $"Restored the backup from {backup.DateText} (demo - no data was changed).";
    }

    // ===== Export =====

    private const string Accounts = "Accounts";
    private const string Services = "Services";
    private const string Employees = "Employees";

    /// <summary>What to export. Each checked item becomes its own CSV file.</summary>
    public List<CheckItem> ExportItems { get; }

    /// <summary>Include inactive accounts and terminated employees.</summary>
    [ObservableProperty]
    public partial bool IncludeInactive { get; set; }

    [RelayCommand]
    private void Export()
    {
        List<CheckItem> chosen = ExportItems.Where(i => i.IsChecked).ToList();
        if (chosen.Count == 0)
        {
            MessageService.Warn("Check at least one thing to export.");
            return;
        }

        OpenFolderDialog dialog = new() { Title = "Choose where to save the exported files" };
        if (dialog.ShowDialog() != true)
            return;

        string stamp = FakeData.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        List<string> written = new();
        try
        {
            foreach (CheckItem item in chosen)
            {
                string path = Path.Combine(dialog.FolderName, $"BillyGoat {item.Name} {stamp}.csv");
                File.WriteAllText(path, BuildCsv(item.Name), new UTF8Encoding(true));   // BOM so Excel reads it as UTF-8
                written.Add(Path.GetFileName(path));
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            MessageService.Warn($"The export could not be saved.\n\n{ex.Message}");
            return;
        }

        StatusMessage = $"Exported {written.Count} file(s) to {dialog.FolderName}.";
        MessageService.Show("Saved:\n" + string.Join("\n", written));
    }

    private string BuildCsv(string what)
    {
        IEnumerable<Account> accounts = FakeData.Accounts.Where(a => IncludeInactive || a.IsActive);
        CsvBuilder csv = new();

        switch (what)
        {
            case Accounts:
                csv.Row("Account", "Status", "Billing name", "Billing address", "Service address",
                        "Home phone", "Mobile phone", "Email", "Route", "Program", "Square feet", "Balance");
                foreach (Account a in accounts)
                    csv.Row(a.AccountNumber, a.Status, a.BillingName, a.BillingAddressText, a.ServiceAddressText,
                            a.HomePhone, a.MobilePhone, a.Email, a.Route, a.Program, a.Measurement.ToString(CultureInfo.InvariantCulture),
                            a.Balance.ToString("0.00", CultureInfo.InvariantCulture));
                break;

            case Services:
                csv.Row("Account", "Invoice", "Service", "Program", "Status", "Date sold", "Date scheduled",
                        "Date completed", "Employee", "Route", "Sale amount", "Adjustments", "Payments", "Balance");
                foreach (Account a in accounts)
                    foreach (ServiceRecord s in a.Services)
                        csv.Row(a.AccountNumber, s.InvoiceNumber, s.ServiceName, s.ProgramName, s.Status,
                                Date(s.DateSold), Date(s.DateScheduled), s.DateCompleted is { } done ? Date(done) : "",
                                s.AssignedEmployee, s.Route,
                                Money(s.SaleAmount), Money(s.Adjustments), Money(s.Payments), Money(s.Balance));
                break;

            case Employees:
                csv.Row("ID", "Last name", "First name", "Title", "Status", "MDARD license");
                foreach (Employee e in FakeData.Employees.Where(e => IncludeInactive || e.IsActive))
                    csv.Row(e.Id, e.LastName, e.FirstName, e.Title, e.StatusText, e.HasMdardLicense ? "Yes" : "No");
                break;
        }
        return csv.ToString();
    }

    private static string Date(DateTime d) => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    private static string Money(decimal m) => m.ToString("0.00", CultureInfo.InvariantCulture);

    /// <summary>Builds CSV text, quoting values that contain commas, quotes or line breaks.</summary>
    private sealed class CsvBuilder
    {
        private readonly StringBuilder _text = new();

        public void Row(params string[] values)
        {
            _text.AppendLine(string.Join(",", values.Select(Quote)));
        }

        private static string Quote(string value) =>
            value.IndexOfAny([',', '"', '\n', '\r']) >= 0 ? $"\"{value.Replace("\"", "\"\"")}\"" : value;

        public override string ToString() => _text.ToString();
    }
}

/// <summary>One line in the backup history list.</summary>
public record BackupRecord(DateTime Date, string User, string Folder, string Kind)
{
    public string DateText => Date.ToString("M/d/yyyy h:mm tt", Formats.Us);
}
