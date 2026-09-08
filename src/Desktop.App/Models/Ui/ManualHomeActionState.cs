using CommunityToolkit.Mvvm.ComponentModel;

namespace Desktop.App.Models.Ui;

public partial class ManualHomeActionState : ObservableObject
{
    public ManualHomeActionState(string title, string subtitle, string commandTagName)
    {
        Title = title;
        Subtitle = subtitle;
        CommandTagName = commandTagName;
    }

    public string Title { get; }

    public string Subtitle { get; }

    public string CommandTagName { get; }

    [ObservableProperty]
    private bool isActive;

    [ObservableProperty]
    private bool isDone;

    [ObservableProperty]
    private bool isCommandActive;

    public string ActivityText => IsActive ? "Đang home" : "Sẵn sàng";

    public string CompletionText => IsDone ? "Done" : "Chưa xong";

    partial void OnIsActiveChanged(bool value)
    {
        OnPropertyChanged(nameof(ActivityText));
    }

    partial void OnIsDoneChanged(bool value)
    {
        OnPropertyChanged(nameof(CompletionText));
    }
}
