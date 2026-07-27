using System.Globalization;
using System.Windows.Data;

namespace Desktop.App.Converters;

public sealed class ResponsiveColumnsConverter : IValueConverter
{
    private const int MinColumns = 4;
    private const int MaxColumns = 6;
    private const double TargetColumnWidth = 300d;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not double width || double.IsNaN(width) || width <= 0)
        {
            return MinColumns;
        }

        var columns = (int)Math.Floor(width / TargetColumnWidth);
        return Math.Clamp(columns, MinColumns, MaxColumns);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
