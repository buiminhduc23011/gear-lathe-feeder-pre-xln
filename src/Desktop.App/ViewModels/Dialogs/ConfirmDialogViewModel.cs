using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Desktop.App.ViewModels.Dialogs;

public partial class ConfirmDialogViewModel : ObservableObject
{
    private readonly TaskCompletionSource<bool> _tcs;
    private readonly Action _close;

    public ConfirmDialogViewModel(
        string title,
        string message,
        TaskCompletionSource<bool> tcs,
        Action close)
    {
        Title = title;
        Message = message;
        _tcs = tcs;
        _close = close;
    }

    public string Title { get; }
    public string Message { get; }

    [RelayCommand]
    private void Confirm()
    {
        _close();
        _tcs.TrySetResult(true);
    }

    [RelayCommand]
    private void Cancel()
    {
        _close();
        _tcs.TrySetResult(false);
    }
}
