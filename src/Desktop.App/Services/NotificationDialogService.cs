using System.Windows;
using Desktop.App.Models.Ui;
using Desktop.App.Services.Abstractions;
using Desktop.App.ViewModels.Dialogs;
using Desktop.App.Views.Dialogs;
using HandyControl.Controls;

namespace Desktop.App.Services;

public sealed class NotificationDialogService : INotificationDialogService
{
    internal static TimeSpan? ResolveAutoDismissAfter(NotificationDialogType type)
    {
        return type is NotificationDialogType.Warning or NotificationDialogType.Info or NotificationDialogType.Success
            ? TimeSpan.FromSeconds(3)
            : null;
    }

    public Task ShowAsync(NotificationDialogType type, string title, string message)
    {
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        Application.Current.Dispatcher.Invoke(() =>
        {
            Dialog? instance = null;
            var autoDismissAfter = ResolveAutoDismissAfter(type);
            var vm = new NotificationDialogViewModel(type, title, message, tcs, () => instance?.Close(), autoDismissAfter);
            var view = new NotificationDialog { DataContext = vm };
            instance = Dialog.Show(view);
        });
        return tcs.Task;
    }

    public Task ShowErrorAsync(string title, string message)
        => ShowAsync(NotificationDialogType.Error, title, message);

    public Task ShowWarningAsync(string title, string message)
        => ShowAsync(NotificationDialogType.Warning, title, message);

    public Task ShowInfoAsync(string title, string message)
        => ShowAsync(NotificationDialogType.Info, title, message);

    public Task ShowSuccessAsync(string title, string message)
        => ShowAsync(NotificationDialogType.Success, title, message);

    public Task<bool> ShowConfirmAsync(string title, string message)
    {
        return ShowConfirmAsync(title, message, CancellationToken.None);
    }

    public Task<bool> ShowConfirmAsync(string title, string message, CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        Application.Current.Dispatcher.Invoke(() =>
        {
            Dialog? instance = null;

            var ctr = cancellationToken.Register(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    instance?.Close();
                    tcs.TrySetCanceled(cancellationToken);
                });
            });

            var vm = new ConfirmDialogViewModel(title, message, tcs, () =>
            {
                ctr.Dispose();
                instance?.Close();
            });

            var view = new ConfirmDialog { DataContext = vm };
            instance = Dialog.Show(view);
        });

        return tcs.Task;
    }
}
