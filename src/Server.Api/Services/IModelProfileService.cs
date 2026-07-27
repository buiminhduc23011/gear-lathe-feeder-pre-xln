using Server.Api.Data.Entities;
using Shared.Models.ModelProfiles;

namespace Server.Api.Services;

public interface IModelProfileService
{
    Task<IReadOnlyList<ModelProfileEntity>> GetByMachineAsync(int machineId, bool includeDeleted = false, CancellationToken cancellationToken = default);

    Task<ModelProfileEntity?> GetByIdAsync(int machineId, int id, CancellationToken cancellationToken = default);

    Task<ModelProfileEntity?> GetByNameAsync(int machineId, string modelName, CancellationToken cancellationToken = default);

    Task<ModelProfileEntity> CreateAsync(int machineId, SaveModelProfileRequest request, string username, CancellationToken cancellationToken = default);

    Task<ModelProfileEntity?> UpdateAsync(int machineId, int id, SaveModelProfileRequest request, string username, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int machineId, int id, string username, CancellationToken cancellationToken = default);

    Task<ModelProfileEntity?> SetEnabledAsync(int machineId, int id, bool isEnabled, string username, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ModelProfileSnapshotEntity>> GetSnapshotsAsync(int modelProfileId, CancellationToken cancellationToken = default);

    Task<byte[]> ExportExcelAsync(int machineId, CancellationToken cancellationToken = default);

    Task<int> UpdateExcelAsync(int machineId, Stream stream, string username, CancellationToken cancellationToken = default);

    Task<int> ReplaceExcelAsync(int machineId, Stream stream, string username, CancellationToken cancellationToken = default);
}
