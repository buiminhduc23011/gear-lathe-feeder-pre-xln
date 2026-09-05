using CommunityToolkit.Mvvm.ComponentModel;
using Desktop.App.Configuration.Plc;

namespace Desktop.App.Models.Ui;

public partial class ManualBinaryOutputState : ObservableObject
{
    public ManualBinaryOutputState(
        string title,
        string subtitle,
        PlcTagDefinition commandTag,
        string activeActionLabel = "Hút",
        string inactiveActionLabel = "Nhả",
        string activeStatusLabel = "Đang hút",
        string inactiveStatusLabel = "Đang nhả")
    {
        Title = title;
        Subtitle = subtitle;
        CommandTag = commandTag;
        ActiveActionLabel = activeActionLabel;
        InactiveActionLabel = inactiveActionLabel;
        ActiveStatusLabel = activeStatusLabel;
        InactiveStatusLabel = inactiveStatusLabel;
    }

    public string Title { get; }

    public string Subtitle { get; }

    public string CommandTagName => CommandTag.Name;

    public string ActiveActionLabel { get; }

    public string InactiveActionLabel { get; }

    public string ActiveStatusLabel { get; }

    public string InactiveStatusLabel { get; }

    internal PlcTagDefinition CommandTag { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    private bool isActive;

    public string StatusText => IsActive ? ActiveStatusLabel : InactiveStatusLabel;
}
