using System.Globalization;
using Desktop.App.Configuration.Plc;

namespace Desktop.App.Services.Plc;

internal static class PlcTagValueTextConverter
{
    public static bool TryParse(PlcTagDefinition definition, string? valueText, out object typedValue, out string errorMessage)
    {
        ArgumentNullException.ThrowIfNull(definition);

        var normalizedText = (valueText ?? string.Empty).Trim();

        switch (definition.DataType)
        {
            case PlcTagDataType.Int16:
                if (short.TryParse(normalizedText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var int16Value))
                {
                    typedValue = int16Value;
                    errorMessage = string.Empty;
                    return true;
                }

                typedValue = default(short);
                errorMessage = "Giá trị phải là số nguyên 16-bit.";
                return false;

            case PlcTagDataType.Int32:
                if (int.TryParse(normalizedText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var int32Value))
                {
                    typedValue = int32Value;
                    errorMessage = string.Empty;
                    return true;
                }

                typedValue = default(int);
                errorMessage = "Giá trị phải là số nguyên 32-bit.";
                return false;

            case PlcTagDataType.Float:
                if (float.TryParse(normalizedText, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var floatValue))
                {
                    typedValue = floatValue;
                    errorMessage = string.Empty;
                    return true;
                }

                typedValue = default(float);
                errorMessage = "Giá trị phải là số thực.";
                return false;

            case PlcTagDataType.Bool:
                if (bool.TryParse(normalizedText, out var boolValue))
                {
                    typedValue = boolValue;
                    errorMessage = string.Empty;
                    return true;
                }

                if (normalizedText is "0" or "1")
                {
                    typedValue = normalizedText == "1";
                    errorMessage = string.Empty;
                    return true;
                }

                typedValue = default(bool);
                errorMessage = "Giá trị phải là true/false hoặc 0/1.";
                return false;

            case PlcTagDataType.String:
                typedValue = NormalizeString(definition, normalizedText);
                errorMessage = string.Empty;
                return true;

            default:
                typedValue = string.Empty;
                errorMessage = $"Không hỗ trợ kiểu dữ liệu '{definition.DataType}'.";
                return false;
        }
    }

    public static string Format(PlcTagDefinition definition, object? value)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (value is null)
        {
            return string.Empty;
        }

        return definition.DataType switch
        {
            PlcTagDataType.Bool => Convert.ToBoolean(value, CultureInfo.InvariantCulture) ? "true" : "false",
            PlcTagDataType.Int16 => Convert.ToInt16(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture),
            PlcTagDataType.Int32 => Convert.ToInt32(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture),
            PlcTagDataType.Float => FormatSingle(Convert.ToSingle(value, CultureInfo.InvariantCulture)),
            PlcTagDataType.String => NormalizeString(definition, Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty),
            _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty,
        };
    }

    public static bool AreEqual(PlcTagDefinition definition, object? left, object? right)
    {
        ArgumentNullException.ThrowIfNull(definition);

        return definition.DataType switch
        {
            PlcTagDataType.Float => Math.Abs(ToSingle(left) - ToSingle(right)) < 0.001f,
            PlcTagDataType.Int16 => Convert.ToInt16(left ?? default(short), CultureInfo.InvariantCulture)
                == Convert.ToInt16(right ?? default(short), CultureInfo.InvariantCulture),
            PlcTagDataType.Int32 => Convert.ToInt32(left ?? default(int), CultureInfo.InvariantCulture)
                == Convert.ToInt32(right ?? default(int), CultureInfo.InvariantCulture),
            PlcTagDataType.Bool => Convert.ToBoolean(left ?? false, CultureInfo.InvariantCulture)
                == Convert.ToBoolean(right ?? false, CultureInfo.InvariantCulture),
            PlcTagDataType.String => string.Equals(
                NormalizeString(definition, Convert.ToString(left, CultureInfo.InvariantCulture) ?? string.Empty),
                NormalizeString(definition, Convert.ToString(right, CultureInfo.InvariantCulture) ?? string.Empty),
                StringComparison.Ordinal),
            _ => Equals(left, right),
        };
    }

    public static object CreateDefaultValue(PlcTagDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        return definition.DataType switch
        {
            PlcTagDataType.Bool => false,
            PlcTagDataType.Int16 => (short)0,
            PlcTagDataType.Int32 => 0,
            PlcTagDataType.Float => 0f,
            PlcTagDataType.String => string.Empty,
            _ => string.Empty,
        };
    }

    private static float ToSingle(object? value)
    {
        return Convert.ToSingle(value ?? 0f, CultureInfo.InvariantCulture);
    }

    private static string FormatSingle(float value)
    {
        return value.ToString("0.###", CultureInfo.InvariantCulture);
    }

    private static string NormalizeString(PlcTagDefinition definition, string value)
    {
        var maxLength = definition.Length ?? definition.Lenght;
        if (maxLength is null || value.Length <= maxLength.Value)
        {
            return value;
        }

        return value[..maxLength.Value];
    }
}
