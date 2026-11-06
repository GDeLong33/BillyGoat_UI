using BillyGoat_UI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;

namespace BillyGoat_UI.ViewModels;

/// <summary>
/// Setup tab. Left: the custom field lists. Right: the items of the selected list,
/// which can be added, renamed and removed. Because every drop-down in the app uses
/// the same lists (Services/Lookups.cs), a change here shows up everywhere at once.
/// Below: the Settings panel (paper size, company name, logo).
/// </summary>
public partial class SetupViewModel : ObservableObject
{
    public SetupViewModel()
    {
        EditText = "";
        SelectedList = Lists[0];
    }

    public List<LookupList> Lists => Lookups.All;

    /// <summary>The shared settings (company name is printed on every report).</summary>
    public AppSettings Settings => AppSettings.Current;
    public string[] PaperSizes => Lookups.PaperSizes;

    [ObservableProperty]
    public partial LookupList? SelectedList { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RenameCommand), nameof(RemoveCommand))]
    public partial string? SelectedItem { get; set; }

    /// <summary>The text box used for Add and Rename.</summary>
    [ObservableProperty] public partial string EditText { get; set; }

    partial void OnSelectedListChanged(LookupList? value) => SelectedItem = value?.Items.FirstOrDefault();

    // Selecting an item puts its text in the box, ready to rename.
    partial void OnSelectedItemChanged(string? value) => EditText = value ?? "";

    /// <summary>Routes are special: each one is also a Route object on the Routing tab.</summary>
    private bool IsRoutes => SelectedList?.Items == Lookups.Routes;
    private bool HasItem() => SelectedItem != null;

    [RelayCommand]
    private void Add()
    {
        if (SelectedList == null || !IsNewValueOk(out string value)) return;

        if (IsRoutes)
        {
            // A new route also appears on the Routing tab (with the next route number).
            value = FakeData.AddRoute(StripRouteNumber(value)).DisplayName;
        }
        SelectedList.Items.Add(value);
        SelectedItem = value;
    }

    [RelayCommand(CanExecute = nameof(HasItem))]
    private void Rename()
    {
        if (SelectedList == null || !IsNewValueOk(out string value)) return;
        string oldValue = SelectedItem!;

        if (IsRoutes)
        {
            Models.Route route = FakeData.Routes.First(r => r.DisplayName == oldValue);
            FakeData.RenameRoute(route, StripRouteNumber(value));   // also updates the list
            SelectedItem = route.DisplayName;
            return;
        }

        int index = SelectedList.Items.IndexOf(oldValue);
        SelectedList.Items[index] = value;              // every drop-down using this list updates
        FakeData.RenameLookupValue(SelectedList, oldValue, value);   // and so do saved records
        SelectedItem = value;
    }

    [RelayCommand(CanExecute = nameof(HasItem))]
    private void Remove()
    {
        if (SelectedList == null) return;
        string value = SelectedItem!;
        if (!MessageService.Confirm($"Remove \"{value}\" from {SelectedList.Name}?\n\nRecords that already use it keep their value."))
            return;

        if (IsRoutes)
        {
            Models.Route route = FakeData.Routes.First(r => r.DisplayName == value);
            if (route.Stops.Count > 0 && !MessageService.Confirm($"{value} still has {route.Stops.Count} stops. Remove the route anyway?"))
                return;
            FakeData.Routes.Remove(route);
        }
        SelectedList.Items.Remove(value);
        SelectedItem = SelectedList.Items.FirstOrDefault();
    }

    /// <summary>Checks the text box: not empty and not already in the list.</summary>
    private bool IsNewValueOk(out string value)
    {
        value = EditText.Trim();
        if (value.Length == 0)
        {
            MessageService.Warn("Type a name first.");
            return false;
        }
        string check = value;
        if (SelectedList!.Items.Any(i => i.Equals(check, StringComparison.OrdinalIgnoreCase)))
        {
            MessageService.Warn($"\"{value}\" is already in the list.");
            return false;
        }
        return true;
    }

    /// <summary>"5 - Midland East" becomes "Midland East" (route numbers are given automatically).</summary>
    private static string StripRouteNumber(string text)
    {
        int dash = text.IndexOf(" - ", StringComparison.Ordinal);
        return dash > 0 && int.TryParse(text[..dash], out _) ? text[(dash + 3)..] : text;
    }

    // ===== Settings =====

    /// <summary>Company logo "Browse..." button: pick an image file.</summary>
    [RelayCommand]
    private void BrowseLogo()
    {
        OpenFileDialog dialog = new()
        {
            Title = "Choose the company logo",
            Filter = "Image files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|All files (*.*)|*.*",
        };
        if (dialog.ShowDialog() == true)
            Settings.LogoFile = dialog.FileName;
    }
}
