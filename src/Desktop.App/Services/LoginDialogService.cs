using System.Windows;
using Desktop.App.Models.Ui;
using Desktop.App.Services.Abstractions;
using Desktop.App.Services.Api;
using Desktop.App.ViewModels.Dialogs;
using Desktop.App.Views.Dialogs;
using HandyControl.Controls;

namespace Desktop.App.Services;

public sealed class LoginDialogService : ILoginDialogService
{
    private readonly IUserApiClient _userApiClient;

    public LoginDialogService(IUserApiClient userApiClient)
    {
        _userApiClient = userApiClient;
    }

    public Task<LoginResult?> ShowAsync()
    {
        var tcs = new TaskCompletionSource<LoginResult?>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        Application.Current.Dispatcher.Invoke(() =>
        {
            Dialog? instance = null;
            var vm = new LoginDialogViewModel(
                _userApiClient,
                tcs,
                () => instance?.Close());
            var view = new LoginDialog { DataContext = vm };
            instance = Dialog.Show(view);
        });

        return tcs.Task;
    }
}
