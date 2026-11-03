using System.Windows;
using BillyGoat_UI.Models;
using BillyGoat_UI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BillyGoat_UI.ViewModels;

/// <summary>
/// ViewModel for the main window. It handles navigation between the tabs.
///
/// How navigation works:
///   1. Each tab RadioButton in MainWindow.xaml is bound to SelectedPage.
///   2. Clicking a tab changes SelectedPage.
///   3. OnSelectedPageChanged then sets CurrentPage to that tab's ViewModel.
///   4. MainWindow shows CurrentPage in a ContentControl, and a DataTemplate picks
///      the matching UserControl (WelcomeViewModel -> WelcomeView, and so on).
/// </summary>
public partial class MainViewModel : ObservableObject
{
    // One ViewModel per tab. They are created once, so a page keeps its state
    // (search text, selected row...) when you switch tabs and come back.
    private readonly Dictionary<AppPage, object> _pages;

    public MainViewModel()
    {
        _pages = new Dictionary<AppPage, object>
        {
            [AppPage.Welcome] = new WelcomeViewModel(),
            [AppPage.CustomerSearch] = new PlaceholderViewModel("Customer Search"),
            [AppPage.Office] = new PlaceholderViewModel("Office"),
            [AppPage.Employees] = new EmployeesViewModel(goHome: () => SelectedPage = AppPage.Welcome),
            [AppPage.ReportGallery] = new PlaceholderViewModel("Report Gallery"),
            [AppPage.Reminders] = new PlaceholderViewModel("Reminders"),
            [AppPage.Marketing] = new PlaceholderViewModel("Marketing"),
            [AppPage.Setup] = new PlaceholderViewModel("Setup"),
        };

        // Start on the Welcome tab.
        SelectedPage = AppPage.Welcome;
        CurrentPage = _pages[AppPage.Welcome];
    }

    /// <summary>The tab that is currently active.</summary>
    [ObservableProperty]
    public partial AppPage SelectedPage { get; set; }

    /// <summary>The ViewModel of the page shown in the main panel.</summary>
    [ObservableProperty]
    public partial object CurrentPage { get; set; }

    // Called automatically by the MVVM Toolkit whenever SelectedPage changes.
    partial void OnSelectedPageChanged(AppPage value)
    {
        CurrentPage = _pages[value];
    }

    /// <summary>Menu: Window > Welcome.</summary>
    [RelayCommand]
    private void GoHome()
    {
        SelectedPage = AppPage.Welcome;
    }

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
