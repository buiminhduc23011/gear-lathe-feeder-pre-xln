using Server.Api.Contracts.Requests;
using Server.Api.Data.Entities;

namespace Server.Api.Services;

public interface IMachineService
{
    Task<IReadOnlyList<MachineEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<MachineEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<MachineEntity> CreateAsync(CreateMachineRequest request, CancellationToken cancellationToken = default);

    Task<MachineEntity?> UpdateAsync(int id, UpdateMachineRequest request, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
