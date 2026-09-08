using Desktop.App.Models.Ui;

namespace Desktop.App.Services.Api;

public interface IUserApiClient
{
    Task<LoginResult> LoginAsync(
        string userName,
        string password,
        CancellationToken cancellationToken = default);
}
