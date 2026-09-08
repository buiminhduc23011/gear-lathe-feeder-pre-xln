using System.Globalization;
using System.Windows.Data;

namespace Desktop.App.Converters;

public sealed class BooleanAndMultiConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values is null || values.Length == 0)
        {
            return false;
        }

        foreach (var value in values)
        {
            if (value is not bool boolValue || !boolValue)
            {
                return false;
            }
        }

        return true;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
