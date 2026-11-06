using CommunityToolkit.Mvvm.ComponentModel;

namespace BillyGoat_UI.Models;

/// <summary>
/// One line in a checklist (e.g. the Programs or Services checklists on the Office tab).
/// Tag can hold the object the line stands for, such as a ServiceRecord.
/// </summary>
public partial class CheckItem : ObservableObject
{
    public CheckItem(string name, bool isChecked = false, object? tag = null)
    {
        Name = name;
        IsChecked = isChecked;
        Tag = tag;
    }

    public string Name { get; }
    public object? Tag { get; }

    [ObservableProperty]
    public partial bool IsChecked { get; set; }
}
