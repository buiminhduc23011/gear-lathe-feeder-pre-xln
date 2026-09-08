using System.Globalization;
using System.Windows.Data;
using Desktop.App.Models.Ui;

namespace Desktop.App.Converters;

/// <summary>
/// Returns "Active" when the bound <see cref="PageType"/> value matches the
/// ConverterParameter string (e.g. "Auto"), otherwise returns an empty string.
/// Used by NavSidebar to set the NavButton Tag so the active-page trigger fires.
/// </summary>
public sealed class ActivePageConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is PageType page
            && Enum.TryParse<PageType>(parameter?.ToString(), out var target)
            && page == target)
        {
            return "Active";
        }

        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
