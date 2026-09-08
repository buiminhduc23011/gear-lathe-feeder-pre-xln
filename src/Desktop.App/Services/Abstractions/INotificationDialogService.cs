using Desktop.App.Models.Ui;

namespace Desktop.App.Services.Abstractions;

public interface INotificationDialogService
{
    Task ShowAsync(NotificationDialogType type, string title, string message);
    Task ShowErrorAsync(string title, string message);
    Task ShowWarningAsync(string title, string message);
    Task ShowInfoAsync(string title, string message);
    Task ShowSuccessAsync(string title, string message);
    Task<bool> ShowConfirmAsync(string title, string message);
    Task<bool> ShowConfirmAsync(string title, string message, CancellationToken cancellationToken);
}
