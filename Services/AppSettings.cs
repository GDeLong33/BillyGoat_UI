using CommunityToolkit.Mvvm.ComponentModel;

namespace BillyGoat_UI.Services;

/// <summary>
/// Settings from the Setup tab's Settings panel. There is only one copy
/// (AppSettings.Current), so the print preview always uses the latest values.
/// </summary>
public partial class AppSettings : ObservableObject
{
    public static AppSettings Current { get; } = new();

    private AppSettings()
    {
        CompanyName = "Billy Goat Lawn Care LLC";
        PaperSize = "Letter";
        LogoFile = "";
    }

    /// <summary>Shown at the top of every printout.</summary>
    [ObservableProperty]
    public partial string CompanyName { get; set; }

    /// <summary>Letter, Legal or A4.</summary>
    [ObservableProperty]
    public partial string PaperSize { get; set; }

    /// <summary>Full path of the chosen logo image, or empty.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LogoFileName))]
    public partial string LogoFile { get; set; }

    /// <summary>Just the file name of the logo, e.g. "goat.png".</summary>
    public string LogoFileName => LogoFile.Length > 0 ? System.IO.Path.GetFileName(LogoFile) : "No logo chosen";
}
