using System.Windows;
using System.Windows.Controls;

namespace BillyGoat_UI.Views;

/// <summary>
/// A boxed panel with a light green header bar, used for every panel on a page
/// (for example "1  Customer lookup"). It is a HeaderedContentControl, so it has a
/// Header (the title) and Content (the body). We add one extra property, Number:
/// when it is set, a small dark green circle with the number is shown before the title.
/// The look is defined in Resources/Styles.xaml.
/// </summary>
public class SectionPanel : HeaderedContentControl
{
    public static readonly DependencyProperty NumberProperty =
        DependencyProperty.Register(nameof(Number), typeof(string), typeof(SectionPanel),
            new PropertyMetadata(null));

    /// <summary>Optional number shown in a circle before the header text.</summary>
    public string? Number
    {
        get => (string?)GetValue(NumberProperty);
        set => SetValue(NumberProperty, value);
    }
}
