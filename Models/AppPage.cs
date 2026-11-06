namespace BillyGoat_UI.Models;

/// <summary>
/// The pages of the main window. The first seven are the tabs in the left rail,
/// in the order they are shown. Account is not a tab: it is the screen shown
/// when a customer account is opened from Search.
/// MainViewModel.SelectedPage holds one of these values.
/// </summary>
public enum AppPage
{
    Search,
    Office,
    Employees,
    ReportGallery,
    Routing,
    BackupExport,
    Setup,
    Account
}
