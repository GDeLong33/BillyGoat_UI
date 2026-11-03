namespace BillyGoat_UI.ViewModels;

/// <summary>
/// ViewModel for tabs that are not built yet. It only holds the tab name;
/// Views/PlaceholderView.xaml shows the name and "Coming soon".
/// </summary>
public class PlaceholderViewModel
{
    public PlaceholderViewModel(string title)
    {
        Title = title;
    }

    public string Title { get; }
}
