using System.Globalization;

namespace Desktop.App.Models.Ui;

internal static class ManualNumeric
{
    public static string Format(float value)
    {
        return value.ToString("0.###", CultureInfo.InvariantCulture);
    }

    public static bool TryParse(string? text, out float value)
    {
        const NumberStyles styles = NumberStyles.Float | NumberStyles.AllowThousands;

        if (float.TryParse(text, styles, CultureInfo.CurrentCulture, out value))
        {
            return true;
        }

        return float.TryParse(text, styles, CultureInfo.InvariantCulture, out value);
    }

    public static bool AreClose(float left, float right)
    {
        return Math.Abs(left - right) < 0.0001f;
    }
}
