using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Desktop.App.Models.Ui;
using System.Windows.Threading;

namespace Desktop.App.ViewModels.Dialogs;
public partial class NotificationDialogViewModel : ObservableObject
{
    private readonly TaskCompletionSource<bool> _tcs;
    private readonly Action _close;
    private readonly DispatcherTimer? _autoDismissTimer;

    public NotificationDialogViewModel(
        NotificationDialogType type,
        string title,
        string message,
        TaskCompletionSource<bool> tcs,
        Action close,
        TimeSpan? autoDismissAfter = null)
    {
        Type = type;
        Title = title;
        Message = message;
        _tcs = tcs;
        _close = close;

        if (autoDismissAfter.HasValue && autoDismissAfter.Value > TimeSpan.Zero)
        {
            _autoDismissTimer = new DispatcherTimer
            {
                Interval = autoDismissAfter.Value
            };
            _autoDismissTimer.Tick += (_, _) => Dismiss();
            _autoDismissTimer.Start();
        }
    }

    public NotificationDialogType Type { get; }
    public string Title { get; }
    public string Message { get; }

    [RelayCommand]
    private void Dismiss()
    {
        _autoDismissTimer?.Stop();
        _close();
        _tcs.TrySetResult(true);
    }
}
