using Shared.Models.ModelProfiles;

namespace Desktop.App.Services.Api;

public interface IModelProfileApiClient
{
    /// <summary>Resolve machineId từ machineCode. Kết quả được cache lại.</summary>
    Task<int> ResolveMachineIdAsync(string machineCode, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ModelProfileDto>> GetAllAsync(int machineId, CancellationToken cancellationToken = default);

    Task<ModelProfileDto?> GetByIdAsync(int machineId, int profileId, CancellationToken cancellationToken = default);

    Task<ModelProfileDto?> GetByNameAsync(int machineId, string modelName, CancellationToken cancellationToken = default);

    Task<ModelProfileDto> CreateAsync(
        int machineId,
        SaveModelProfileRequest request,
        CancellationToken cancellationToken = default,
        string? accessTokenOverride = null);

    Task<ModelProfileDto> UpdateAsync(
        int machineId,
        int profileId,
        SaveModelProfileRequest request,
        CancellationToken cancellationToken = default,
        string? accessTokenOverride = null);

    Task DeleteAsync(
        int machineId,
        int profileId,
        CancellationToken cancellationToken = default,
        string? accessTokenOverride = null);
}
