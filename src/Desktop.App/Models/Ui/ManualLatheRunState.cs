using CommunityToolkit.Mvvm.ComponentModel;
using Desktop.App.Configuration.Plc;

namespace Desktop.App.Models.Ui;

public partial class ManualLatheRunState : ObservableObject
{
    public ManualLatheRunState(
        string title,
        string subtitle,
        PlcTagDefinition runCommandTag,
        PlcTagDefinition runningFeedbackTag)
    {
        Title = title;
        Subtitle = subtitle;
        RunCommandTag = runCommandTag;
        RunningFeedbackTag = runningFeedbackTag;
    }

    public string Title { get; }

    public string Subtitle { get; }

    public string CommandTagName => RunCommandTag.Name;

    internal PlcTagDefinition RunCommandTag { get; }

    internal PlcTagDefinition RunningFeedbackTag { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    private bool isRunning;

    public string StatusText => IsRunning ? "Đang chạy" : "Đang dừng";
}
