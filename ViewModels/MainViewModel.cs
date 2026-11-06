using System.Windows;
using BillyGoat_UI.Models;
using BillyGoat_UI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BillyGoat_UI.ViewModels;

/// <summary>
/// ViewModel for the main window. It handles signing in/out and navigation.
///
/// How navigation works:
///   1. Each tab RadioButton in MainWindow.xaml is bound to SelectedPage.
///   2. Clicking a tab changes SelectedPage.
///   3. OnSelectedPageChanged then sets CurrentPage to that tab's ViewModel.
///   4. MainWindow shows CurrentPage in a ContentControl, and a DataTemplate
///      (Resources/ViewTemplates.xaml) picks the matching UserControl.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    // One ViewModel per tab. They are created at sign-in, so a page keeps its state
    // (search text, selected row...) when you switch tabs and come back.
    private readonly Dictionary<AppPage, object> _pages = new();

    public MainViewModel()
    {
        Login = new LoginViewModel(OnSignedIn);
    }

    // ===== Signing in and out =====

    /// <summary>The login screen's ViewModel (shown while nobody is signed in).</summary>
    [ObservableProperty]
    public partial LoginViewModel Login { get; set; }

    /// <summary>The signed-in user, or null on the login screen.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSignedIn))]
    [NotifyPropertyChangedFor(nameof(SignedInName))]
    [NotifyPropertyChangedFor(nameof(SignedInRole))]
    public partial User? CurrentUser { get; set; }

    public bool IsSignedIn => CurrentUser != null;
    public string SignedInName => CurrentUser?.FullName ?? "";
    public string SignedInRole => CurrentUser?.Role.ToString() ?? "";

    /// <summary>LoginViewModel calls this after a correct user ID and password.</summary>
    private void OnSignedIn(User user)
    {
        CurrentUser = user;
        CreatePages();
        SelectedPage = AppPage.Search;
        CurrentPage = _pages[AppPage.Search];
    }

    /// <summary>Creates a fresh ViewModel for every tab.</summary>
    private void CreatePages()
    {
        _pages.Clear();
        _pages[AppPage.Search] = new PlaceholderViewModel("Search");
        _pages[AppPage.Office] = new PlaceholderViewModel("Office");
        _pages[AppPage.Employees] = new EmployeesViewModel(goHome: () => SelectedPage = AppPage.Search);
        _pages[AppPage.ReportGallery] = new PlaceholderViewModel("Report Gallery");
        _pages[AppPage.Routing] = new PlaceholderViewModel("Routing");
        _pages[AppPage.BackupExport] = new PlaceholderViewModel("Backup & Export");
        _pages[AppPage.Setup] = new PlaceholderViewModel("Setup");
    }

    /// <summary>"Sign out" link at the bottom of the rail.</summary>
    [RelayCommand]
    private void SignOut()
    {
        if (!MessageService.Confirm("Sign out of Billy Goat Lawn Care?"))
            return;

        CurrentUser = null;
        _pages.Clear();
        Login = new LoginViewModel(OnSignedIn);   // a new, empty login form
        CurrentPage = null;
    }

    // ===== Navigation =====

    /// <summary>The tab that is currently active.</summary>
    [ObservableProperty]
    public partial AppPage SelectedPage { get; set; }

    /// <summary>The ViewModel of the page shown in the main panel.</summary>
    [ObservableProperty]
    public partial object? CurrentPage { get; set; }

    // Called automatically by the MVVM Toolkit whenever SelectedPage changes.
    partial void OnSelectedPageChanged(AppPage value)
    {
        if (_pages.TryGetValue(value, out object? page))
            CurrentPage = page;
    }

    // ===== Menu bar =====

    /// <summary>Menu: File > Exit.</summary>
    [RelayCommand]
    private void ExitApp()
    {
        Application.Current.Shutdown();
    }

    /// <summary>Menu: Help > About.</summary>
    [RelayCommand]
    private void ShowAbout()
    {
        MessageService.Show("Billy Goat Lawn Care\nDemo interface (sample data only).");
    }
}
