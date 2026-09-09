using Desktop.App.Models.Agv;
using Desktop.App.Services.Api;

namespace Desktop.App.Services.Abstractions;

public interface IRobotCurrentOrderService
{
    void AttachModelProfileApiClient(IModelProfileApiClient modelProfileApiClient);
    Task MarkSwapRequestedAsync(AgvPosition machineSlot);
    Task MarkSwapCompletedAsync(AgvPosition machineSlot);
    Task LoadCurrentOrderAsync(AgvPosition machineSlot, int shelfProductCount, int shelfOrderCount, int shelfLayoutType, AgvOrderData currentOrder);
    Task SetProductionResultAcknowledgedAsync(AgvPosition machineSlot, bool acknowledged);
    Task ClearCurrentOrderAsync(AgvPosition machineSlot, bool setClearRequested = true, bool clearCompletedBit = true);
    Task SetShelfOrdersCompletedAsync(AgvPosition machineSlot, bool completed);
    Task ReloadModelParametersAsync(AgvPosition machineSlot, AgvOrderData currentOrder);
    Task SetPauseInspectAsync(bool pause);
}