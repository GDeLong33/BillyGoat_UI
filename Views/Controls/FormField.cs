using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;

namespace BillyGoat_UI.Views;

/// <summary>
/// A label shown above one input, for example:
///     &lt;views:FormField Header="First name"&gt;&lt;TextBox Text="{Binding FirstName}" /&gt;&lt;/views:FormField&gt;
///
/// Accessibility: screen readers need a name for every input. So when the input
/// does not already have AutomationProperties.Name, we copy the Header text into it.
/// That way each label only has to be typed once.
/// The look is defined in Resources/Styles.xaml.
/// </summary>
public class FormField : HeaderedContentControl
{
    protected override void OnContentChanged(object oldContent, object newContent)
    {
        base.OnContentChanged(oldContent, newContent);
        CopyHeaderToInput();
    }

    protected override void OnHeaderChanged(object oldHeader, object newHeader)
    {
        base.OnHeaderChanged(oldHeader, newHeader);
        CopyHeaderToInput();
    }

    private void CopyHeaderToInput()
    {
        if (Content is DependencyObject input && Header is string label
            && string.IsNullOrEmpty(AutomationProperties.GetName(input)))
        {
            AutomationProperties.SetName(input, label);
        }
    }
}
