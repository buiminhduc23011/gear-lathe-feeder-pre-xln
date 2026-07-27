using Desktop.App.Configuration.Plc;
using Desktop.App.Models.Agv;
using Desktop.App.Services.Abstractions;
using Desktop.App.Services.Api;

namespace Desktop.App.Services.Robot;

internal sealed class RobotCurrentOrderService : IRobotCurrentOrderService
{
    private readonly IPlcService _plcService;
    private readonly RobotCurrentOrderParameterWriter _parameterWriter;

    public RobotCurrentOrderService(IPlcService plcService, RobotCurrentOrderParameterWriter parameterWriter)
    {
        _plcService = plcService;
        _parameterWriter = parameterWriter;
    }

    public void AttachModelProfileApiClient(IModelProfileApiClient modelProfileApiClient)
    {
        _parameterWriter.AttachModelProfileApiClient(modelProfileApiClient);
    }

    public Task MarkSwapRequestedAsync(AgvPosition machineSlot)
    {
        var tag = machineSlot == AgvPosition.Position1
            ? PlcTagCatalog.Agv.AgvRequestSwapLine1
            : PlcTagCatalog.Agv.AgvRequestSwapLine2;

        return _plcService.WriteAsync(tag.Name, true);
    }

    public Task MarkSwapCompletedAsync(AgvPosition machineSlot)
    {
        var tag = machineSlot == AgvPosition.Position1
            ? PlcTagCatalog.Agv.AgvSwapDoneLine1
            : PlcTagCatalog.Agv.AgvSwapDoneLine2;

        return _plcService.WriteAsync(tag.Name, true);
    }

    public Task LoadCurrentOrderAsync(AgvPosition machineSlot, int shelfProductCount, int shelfOrderCount, int shelfLayoutType, AgvOrderData currentOrder)
    {
        return _parameterWriter.WriteOrderAsync(machineSlot, shelfProductCount, shelfOrderCount, shelfLayoutType, currentOrder);
    }

    public Task SetProductionResultAcknowledgedAsync(AgvPosition machineSlot, bool acknowledged)
    {
        return _parameterWriter.SetProductionResultAcknowledgedAsync(machineSlot, acknowledged);
    }

    public async Task ClearCurrentOrderAsync(AgvPosition machineSlot, bool setClearRequested = true, bool clearCompletedBit = true)
    {
        await _parameterWriter.ClearOrderAsync(machineSlot, clearCompletedBit);
        if (setClearRequested)
        {
            await _parameterWriter.SetClearRequestedAsync(machineSlot, true);
        }
    }

    public Task SetShelfOrdersCompletedAsync(AgvPosition machineSlot, bool completed)
    {
        var tag = machineSlot == AgvPosition.Position1
            ? PlcTagCatalog.DataAutos.OrderLine1ShelfOrdersCompleted
            : PlcTagCatalog.DataAutos.OrderLine2ShelfOrdersCompleted;

        return _plcService.WriteAsync(tag.Name, completed);
    }
}