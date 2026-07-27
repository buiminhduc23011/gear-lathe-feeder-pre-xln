using Desktop.App.Models.Agv;

namespace Desktop.App.Services.Agv;

public interface IAgvTransferApiService
{
    Task<(AgvTransferStatus Status, string Message, int? DeclarationId)> CheckStatusAsync(AgvPosition position, CancellationToken cancellationToken = default);
    Task<(bool Success, string Message)> ConfirmCommandAsync(AgvPosition position, CancellationToken cancellationToken = default);
    Task<(bool Success, string Message)> CreateCommandAsync(AgvPosition position, CancellationToken cancellationToken = default);
}
