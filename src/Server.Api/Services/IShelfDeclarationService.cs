using Server.Api.Contracts.Requests;
using Server.Api.Contracts.Responses;
using Server.Api.Data.Entities;

namespace Server.Api.Services;

public interface IShelfDeclarationService
{
    Task<ManualShelfDeclarationEntity> CreateAsync(int machineId, CreateShelfDeclarationRequest request, string username, CancellationToken cancellationToken = default);
    Task<List<ManualShelfDeclarationEntity>> GetByMachineAsync(int machineId, CancellationToken cancellationToken = default);
    Task<List<ShelfDeclarationSlotStatusResponse>> GetSlotStatusesAsync(int machineId, CancellationToken cancellationToken = default);
    Task<List<AgvSlotStatusResponse>> GetAgvSlotStatusesAsync(CancellationToken cancellationToken = default);
    Task<AgvCallEligibilityResponse> GetAgvCallEligibilityAsync(string machineCode, int machineSlotIndex, CancellationToken cancellationToken = default);
    Task<AgvPickupResponse?> PickupForAgvAsync(AgvPickupRequest request, CancellationToken cancellationToken = default);
    Task<MachineLoadResponse?> GetMachineLoadAsync(int id, string machineCode, CancellationToken cancellationToken = default);
    Task<MachineLoadResponse?> GetPendingManualLoadAsync(string machineCode, int machineSlotIndex, CancellationToken cancellationToken = default);
    Task<MachineLoadResponse?> GetActiveLoadAsync(string machineCode, int machineSlotIndex, CancellationToken cancellationToken = default);
    Task<bool> RequestLoadAsync(int id, string username, CancellationToken cancellationToken = default);
    Task<List<ManualShelfDeclarationEntity>> GetLoadRequestsByMachineCodeAsync(string machineCode, CancellationToken cancellationToken = default);
    Task<ManualShelfDeclarationEntity?> ApplyMachineEventAsync(int id, MachineShelfEventRequest request, CancellationToken cancellationToken = default);
    Task<List<ShelfDeclarationEventEntity>> GetEventsAsync(int id, CancellationToken cancellationToken = default);
    Task<ProductionLifecycleReportResponse> GetProductionLifecycleReportAsync(
        int? machineId,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        string? status,
        int? shelfIndex,
        CancellationToken cancellationToken = default);
    Task<bool> CancelAsync(int id, string username, CancellationToken cancellationToken = default);
}
