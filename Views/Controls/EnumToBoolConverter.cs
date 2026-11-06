using System.Globalization;
using System.Windows.Data;

namespace BillyGoat_UI.Views;

/// <summary>
/// Lets a group of RadioButtons bind to a single enum property.
/// Each RadioButton passes "its" enum value as the ConverterParameter:
///   - Convert:     the button is checked when the property equals its value.
///   - ConvertBack: when the user checks the button, the property is set to its value.
///                  When a button becomes unchecked we do nothing (another button in
///                  the group is being checked and will set the value instead).
/// </summary>
public class EnumToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value != null && value.Equals(parameter);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is true ? parameter : Binding.DoNothing;
    }
}
