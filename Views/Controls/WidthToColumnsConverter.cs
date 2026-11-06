using System.Globalization;
using System.Windows.Data;

namespace BillyGoat_UI.Views;

/// <summary>
/// "Two columns when there is room, one column on a narrow window."
/// Used with a UniformGrid that binds its Columns to its own width:
///     Columns="{Binding ActualWidth, RelativeSource={RelativeSource Self},
///               Converter={StaticResource WidthToColumns}, ConverterParameter=900}"
/// The parameter is the smallest width (in pixels) that still gets two columns.
/// </summary>
public class WidthToColumnsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        double width = value is double w ? w : 0;
        double minWidth = double.TryParse(parameter as string, NumberStyles.Any, CultureInfo.InvariantCulture, out double p) ? p : 900;
        return width >= minWidth || width == 0 ? 2 : 1;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
