using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Desktop.App.Models.Ui;

/// <summary>One option in a select/combo-box field.</summary>
public sealed record SelectOption(int Value, string Label)
{
    public override string ToString() => Label;
}

/// <summary>Shared Jig-type options used by both Robot and Line field catalogs.</summary>
public static class JigTypeOptions
{
    public static readonly SelectOption[] Items =
    [
        new(0, "0: Không xác định"),
        new(1, "1: Tay kẹp nhỏ"),
        new(2, "2: Tay kẹp to rộng 12mm"),
        new(3, "3: Tay kẹp to rộng 25mm"),
    ];
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
        // Cải tiến theo yêu cầu mr.Tùng ngày 27/04/2026: Ẩn các điểm check gốc robot
        /*
        new("originCheck1X",      "Tọa độ X gốc check 1"),
        new("originCheck1Y",      "Tọa độ Y gốc check 1"),
        new("originCheck1Z",      "Tọa độ Z gốc check 1"),
        new("originCheck2X",      "Tọa độ X gốc check 2"),
        new("originCheck2Y",      "Tọa độ Y gốc check 2"),
        new("originCheck2Z",      "Tọa độ Z gốc check 2"),
        */
        new("jigProductHeight",   "Độ cao trên Jig"),
        new("jigCenterOffset",    "Ofset Tâm Jig"),
        new("jigDepthOffset",     "Ofset độ cao âm xuống Jig (Không tính OP2)"),
        new("modelJigClampType",  "Model Jig tay kẹp",  "int", JigTypeOptions.Items),
    ];

    public static readonly ModelFieldDefinition[] LineFields =
    [
        new("jigType",         "Loại tay kẹp",     "int", JigTypeOptions.Items),
        new("pickInputX",      "Tọa độ X gắp SP đầu vào line"),
        new("pickInputZ",      "Tọa độ Z gắp SP đầu vào line"),
        new("pickOp1X",        "Tọa độ X an toàn lên xuống Op1"),
        new("pickOp1Z",        "Tọa độ Z an toàn lên xuống Op1"),
        new("pickOp2X",        "Tọa độ X an toàn lên xuống Op2"),
        new("pickOp2Z",        "Tọa độ Z an toàn lên xuống Op2"),
        new("placeOp1X",       "Tọa độ X chống tâm Op1"),
        new("placeOp1Z",       "Tọa độ Z chống tâm Op1"),
        new("placeOp2X",       "Tọa độ X chống tâm Op2"),
        new("placeOp2Z",       "Tọa độ Z chống tâm Op2"),
        new("placeMeasureX",   "Tọa độ X chống tâm máy đo"),
        new("placeMeasureZ",   "Tọa độ Z chống tâm máy đo"),
        new("jigProductHeight","Tọa độ Jig đỡ trục đầu vào"),
        new("grindingTimeOp1", "Thời gian mài Op1",                "int"),
        new("grindingTimeOp2", "Thời gian mài Op2",                "int"),
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
