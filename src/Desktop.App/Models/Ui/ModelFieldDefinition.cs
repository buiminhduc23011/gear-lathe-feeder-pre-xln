using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Desktop.App.Models.Ui;

/// <summary>One option in a select/combo-box field.</summary>
public sealed record SelectOption(int Value, string Label)
{
    public override string ToString() => Label;
}

/// <summary>Shared Jig-type options used by both Robot and Line field catalogs.</summary>
public static class JigSupplyTypeOptions
{
    public static readonly SelectOption[] Items =
    [
        new(0, "0: Không xác định"),
        new(1, "1: Jig Phi 20"),
        new(2, "2: Jig Phi 30"),
        new(3, "3: Jig Phi 40"),
        new(4, "4: Jig có thể điều chỉnh"),
    ];
}

public static class JigTypeOptions
{
    public static SelectOption[] Items => JigSupplyTypeOptions.Items;
}

public sealed record ModelFieldDefinition(
    string Key,
    string Label,
    string FieldType = "real",
    SelectOption[]? Options = null)
{
    /// <summary>True when this field should render as a ComboBox.</summary>
    public bool IsSelectField => Options is { Length: > 0 };
}

public static class ModelFieldCatalog
{
    public static readonly ModelFieldDefinition[] RobotFields =
    [
        new("inputBlankThickness",               "Độ dày Phôi đầu vào"),
        new("outerFinishedDiameter",            "Đường kính ngoài phôi thành phẩm"),
        new("op1TurnedThickness",                "Độ dày phôi sau tiện OP1"),
        new("finishedThickness",                 "Độ dày Phôi thành phẩm"),
        new("pickDropZOffset",                   "Ofset tọa độ Z gắp thả hàng"),
        new("chuckStepDepth",                    "Chiều sâu bậc mâm cặp"),
        new("innerFinishedDiameter",             "Đường kính trong phôi thành phẩm"),
        new("innerDiameterToGDiameterDistance",  "Khoảng cách đường kính trong đến đường kính G"),
        new("magnetCount",                       "Số nam châm sử dụng", "int"),
        new("jigSupplyType",                     "Loại Jig cấp hàng", "int", JigSupplyTypeOptions.Items),
    ];
}

public sealed partial class ModelFieldValue : ObservableObject
{
    public ModelFieldDefinition Definition { get; }
    public bool IsReadOnly { get; }

    [ObservableProperty]
    private string _valueText = string.Empty;

    [ObservableProperty]
    private string _validationMessage = string.Empty;

    [ObservableProperty]
    private SelectOption? _selectedOption;

    public string Label => Definition.Label;
    public string Key   => Definition.Key;

    /// <summary>True when this field should render as a ComboBox.</summary>
    public bool IsSelectField => Definition.IsSelectField;

    /// <summary>Options list for binding to ComboBox ItemsSource.</summary>
    public ReadOnlyCollection<SelectOption>? Options =>
        Definition.Options is not null
            ? new ReadOnlyCollection<SelectOption>(Definition.Options)
            : null;

    /// <summary>Friendly display text for read-only mode (Line tabs).</summary>
    public string DisplayText
    {
        get
        {
            if (IsSelectField && int.TryParse(ValueText, out var intVal))
            {
                var match = Definition.Options?.FirstOrDefault(o => o.Value == intVal);
                return match?.Label ?? ValueText;
            }
            return ValueText;
        }
    }

    /// <summary>True when the field key ends with X, Y or Z — eligible for position capture from PLC.</summary>
    public bool IsCoordinateField =>
        Key.EndsWith("X", StringComparison.Ordinal) ||
        Key.EndsWith("Y", StringComparison.Ordinal) ||
        Key.EndsWith("Z", StringComparison.Ordinal);

    public bool HasValidationMessage => !string.IsNullOrEmpty(ValidationMessage);

    public ModelFieldValue(ModelFieldDefinition definition, bool isReadOnly = false)
    {
        Definition = definition;
        IsReadOnly = isReadOnly;
    }

    partial void OnValueTextChanged(string value)
    {
        OnPropertyChanged(nameof(DisplayText));

        // Sync SelectedOption when ValueText changes (e.g. from API load)
        if (IsSelectField && int.TryParse(value, out var intVal))
        {
            var match = Definition.Options?.FirstOrDefault(o => o.Value == intVal);
            if (match != SelectedOption)
            {
                _selectedOption = match;
                OnPropertyChanged(nameof(SelectedOption));
            }
        }
    }

    partial void OnSelectedOptionChanged(SelectOption? value)
    {
        // Sync ValueText when user changes ComboBox selection
        if (value is not null)
        {
            var newText = value.Value.ToString();
            if (ValueText != newText)
            {
                ValueText = newText;
            }
        }
    }

    partial void OnValidationMessageChanged(string value)
    {
        OnPropertyChanged(nameof(HasValidationMessage));
    }
}
