using CommunityToolkit.Mvvm.ComponentModel;
using Desktop.App.Configuration.Plc;

namespace Desktop.App.Models.Ui;

public partial class ManualBinaryOutputState : ObservableObject
{
    public ManualBinaryOutputState(string title, string subtitle, PlcTagDefinition commandTag)
    {
        Title = title;
        Subtitle = subtitle;
        CommandTag = commandTag;
    }

    public string Title { get; }

    public string Subtitle { get; }

    public string CommandTagName => CommandTag.Name;

    internal PlcTagDefinition CommandTag { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    private bool isActive;

    public string StatusText => IsActive ? "Đang hút" : "Đang nhả";
}
