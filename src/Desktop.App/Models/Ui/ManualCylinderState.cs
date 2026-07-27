using CommunityToolkit.Mvvm.ComponentModel;
using Desktop.App.Configuration.Plc;

namespace Desktop.App.Models.Ui;

public partial class ManualCylinderState : ObservableObject
{
    public ManualCylinderState(
        string key,
        string title,
        string subtitle,
        string primaryActionLabel,
        string secondaryActionLabel,
        string primaryFeedbackLabel,
        string secondaryFeedbackLabel,
        PlcTagDefinition primaryCommandTag,
        PlcTagDefinition secondaryCommandTag,
        PlcTagDefinition primaryFeedbackTag,
        PlcTagDefinition secondaryFeedbackTag)
    {
        Key = key;
        Title = title;
        Subtitle = subtitle;
        PrimaryActionLabel = primaryActionLabel;
        SecondaryActionLabel = secondaryActionLabel;
        PrimaryFeedbackLabel = primaryFeedbackLabel;
        SecondaryFeedbackLabel = secondaryFeedbackLabel;
        PrimaryCommandTag = primaryCommandTag;
        SecondaryCommandTag = secondaryCommandTag;
        PrimaryFeedbackTag = primaryFeedbackTag;
        SecondaryFeedbackTag = secondaryFeedbackTag;
    }

    public string Key { get; }

    public string Title { get; }

    public string Subtitle { get; }

    public string PrimaryActionLabel { get; }

    public string SecondaryActionLabel { get; }

    public string PrimaryFeedbackLabel { get; }

    public string SecondaryFeedbackLabel { get; }

    public string PrimaryCommandTagName => PrimaryCommandTag.Name;

    public string SecondaryCommandTagName => SecondaryCommandTag.Name;

    internal PlcTagDefinition PrimaryCommandTag { get; }

    internal PlcTagDefinition SecondaryCommandTag { get; }

    internal PlcTagDefinition PrimaryFeedbackTag { get; }

    internal PlcTagDefinition SecondaryFeedbackTag { get; }

    [ObservableProperty]
    private bool isPrimaryCommandActive;

    [ObservableProperty]
    private bool isSecondaryCommandActive;

    [ObservableProperty]
    private bool isPrimaryFeedbackActive;

    [ObservableProperty]
    private bool isSecondaryFeedbackActive;

    public string PrimaryFeedbackText => IsPrimaryFeedbackActive ? "ON" : "OFF";

    public string SecondaryFeedbackText => IsSecondaryFeedbackActive ? "ON" : "OFF";

    partial void OnIsPrimaryFeedbackActiveChanged(bool value)
    {
        OnPropertyChanged(nameof(PrimaryFeedbackText));
    }

    partial void OnIsSecondaryFeedbackActiveChanged(bool value)
    {
        OnPropertyChanged(nameof(SecondaryFeedbackText));
    }

    public void ApplyObservedValues(
        bool isPrimaryCommandActive,
        bool isSecondaryCommandActive,
        bool isPrimaryFeedbackActive,
        bool isSecondaryFeedbackActive)
    {
        IsPrimaryCommandActive = isPrimaryCommandActive;
        IsSecondaryCommandActive = isSecondaryCommandActive;
        IsPrimaryFeedbackActive = isPrimaryFeedbackActive;
        IsSecondaryFeedbackActive = isSecondaryFeedbackActive;
    }
}
