using System.Globalization;
using System.Windows.Data;

namespace Desktop.App.Converters;

public class RelativeTimeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not DateTimeOffset dateTime)
        {
            return string.Empty;
        }

        var timeSpan = DateTimeOffset.UtcNow - dateTime;

        if (timeSpan.TotalSeconds < 0)
        {
            return "vừa xong";
        }

        if (timeSpan < TimeSpan.FromSeconds(60))
        {
            return $"{(int)timeSpan.TotalSeconds} giây trước";
        }

        if (timeSpan < TimeSpan.FromMinutes(60))
        {
            return $"{(int)timeSpan.TotalMinutes} phút trước";
        }

        if (timeSpan < TimeSpan.FromHours(24))
        {
            return $"{(int)timeSpan.TotalHours} giờ trước";
        }

        if (timeSpan < TimeSpan.FromDays(30))
        {
            return $"{(int)timeSpan.TotalDays} ngày trước";
        }

        if (timeSpan < TimeSpan.FromDays(365))
        {
            var months = (int)(timeSpan.TotalDays / 30);
            return $"{months} tháng trước";
        }

        return dateTime.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
