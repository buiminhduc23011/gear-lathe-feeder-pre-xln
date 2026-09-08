using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Desktop.App.Models.Ui;
using Desktop.App.Services.Api;

namespace Desktop.App.ViewModels.Dialogs;

public partial class LoginDialogViewModel : ObservableObject
{
    private readonly IUserApiClient _userApiClient;
    private readonly TaskCompletionSource<LoginResult?> _tcs;
    private readonly Action _close;
    private string _password = string.Empty;

    [ObservableProperty]
    private string _userName = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public LoginDialogViewModel(
        IUserApiClient userApiClient,
        TaskCompletionSource<LoginResult?> tcs,
        Action close)
    {
        _userApiClient = userApiClient;
        _tcs = tcs;
        _close = close;
    }

    public void SetPassword(string password) => _password = password;

    [RelayCommand]
    private async Task LoginAsync(CancellationToken cancellationToken)
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(UserName))
        {
            ErrorMessage = "Vui lòng nhập tên đăng nhập.";
            return;
        }

        if (string.IsNullOrWhiteSpace(_password))
        {
            ErrorMessage = "Vui lòng nhập mật khẩu.";
            return;
        }

        IsLoading = true;

        try
        {
            var result = await _userApiClient.LoginAsync(
                UserName.Trim(), _password, cancellationToken);

            if (result.Success)
            {
                _close();
                _tcs.TrySetResult(result);
            }
            else
            {
                ErrorMessage = result.ErrorMessage ?? "Đăng nhập thất bại.";
            }
        }
        catch (OperationCanceledException)
        {
            // Dialog closed while request was in flight
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        _close();
        _tcs.TrySetResult(null);
    }
}
