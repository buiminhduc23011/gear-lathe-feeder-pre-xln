using Desktop.App.Models.Ui;

namespace Desktop.App.Services.Abstractions;

public interface ILoginDialogService
{
    Task<LoginResult?> ShowAsync();
}
