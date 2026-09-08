using CommunityToolkit.Mvvm.ComponentModel;
using Desktop.App.Configuration.Plc;

namespace Desktop.App.Models.Ui;

public partial class EditablePlcParameterField : ObservableObject
{
    public EditablePlcParameterField(PlcTagDefinition definition, string groupName, string valueText)
    {
        Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        GroupName = groupName ?? throw new ArgumentNullException(nameof(groupName));
        valueText = valueText ?? string.Empty;

        tagName = definition.Name;
        label = definition.Description;
        address = definition.Address;
        this.valueText = valueText;
        lastSavedValueText = valueText;
    }

    public PlcTagDefinition Definition { get; }

    public string GroupName { get; }

    [ObservableProperty]
    private string tagName = string.Empty;

    [ObservableProperty]
    private string label = string.Empty;

    [ObservableProperty]
    private string address = string.Empty;

    [ObservableProperty]
    private string valueText = string.Empty;

    [ObservableProperty]
    private bool isSyncedWithPlc;

    [ObservableProperty]
    private string validationMessage = string.Empty;

    [ObservableProperty]
    private string lastSavedValueText = string.Empty;

    public bool HasValidationMessage => !string.IsNullOrWhiteSpace(ValidationMessage);

    public string SyncGlyph => IsSyncedWithPlc ? "✓" : string.Empty;

    partial void OnValidationMessageChanged(string value)
    {
        OnPropertyChanged(nameof(HasValidationMessage));
    }

    partial void OnIsSyncedWithPlcChanged(bool value)
    {
        OnPropertyChanged(nameof(SyncGlyph));
    }
}
