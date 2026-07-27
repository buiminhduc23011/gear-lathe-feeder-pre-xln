using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Desktop.App.Converters;

/// <summary>
/// Converts null/empty string to Visibility.Collapsed, otherwise Visible.
/// </summary>
public class NullToCollapsedConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string s)
            return string.IsNullOrWhiteSpace(s) ? Visibility.Collapsed : Visibility.Visible;

        return value == null ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Converts boolean auto-call pause state to display text.
/// false (not paused) → "Cho phép gọi AGV"
/// true (paused)      → "Disable call AGV"
/// </summary>
public class BoolToAutoCallPauseTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isPaused && isPaused)
            return "TẮT";

        return "BẬT";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Converts a percentage (0-100) to actual width based on container ActualWidth.
/// Used as MultiBinding: value[0] = percent, value[1] = container ActualWidth.
/// </summary>
public class PercentToWidthConverter : IMultiValueConverter
{
    public object Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Length >= 2
            && values[0] is double percent
            && values[1] is double containerWidth)
        {
            return Math.Max(0, containerWidth * Math.Clamp(percent, 0, 100) / 100.0);
        }

        return 0.0;
    }

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Converts IsRunning boolean to RUN/STOP display text.
/// true  → "● RUN"
/// false → "■ STOP"
/// </summary>
public class BoolToRunStopTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isRunning && isRunning)
            return "RUN";

        return "STOP";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Inverse of BooleanToVisibilityConverter.
/// true  → Collapsed
/// false → Visible
/// </summary>
public class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b && b)
            return Visibility.Collapsed;

        return Visibility.Visible;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
