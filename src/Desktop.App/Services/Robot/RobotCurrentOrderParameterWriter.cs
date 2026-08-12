using Desktop.App.Configuration.Plc;
using Desktop.App.Models.Agv;
using Desktop.App.Services.Abstractions;
using Desktop.App.Services.Api;

namespace Desktop.App.Services.Robot;

internal sealed class RobotCurrentOrderParameterWriter
{
    private readonly IPlcService _plcService;
    private readonly RobotTrayPlacementResolver _trayPlacementResolver;
    private readonly RobotJigTypeResolver _jigTypeResolver;

    public RobotCurrentOrderParameterWriter(
        IPlcService plcService,
        RobotTrayPlacementResolver trayPlacementResolver,
        RobotJigTypeResolver jigTypeResolver)
    {
        _plcService = plcService;
        _trayPlacementResolver = trayPlacementResolver;
        _jigTypeResolver = jigTypeResolver;
    }

    public async Task WriteOrderAsync(AgvPosition machineSlot, int shelfProductCount, int shelfOrderCount, int shelfLayoutType, AgvOrderData currentOrder)
    {
        ArgumentNullException.ThrowIfNull(currentOrder);

        if (string.IsNullOrWhiteSpace(currentOrder.OrderId))
        {
            throw new InvalidOperationException("OrderId is required for machine order write.");
        }

        var modelId = string.IsNullOrWhiteSpace(currentOrder.ModelName) 
            ? currentOrder.ModelId?.Trim() ?? string.Empty 
            : currentOrder.ModelName.Trim();

        if (string.IsNullOrWhiteSpace(modelId))
        {
            throw new InvalidOperationException("ModelId/ModelName is required for machine order write.");
        }

        var orderCode = currentOrder.OrderId.Trim();

        var profileLineData = HasProfileSnapshot(currentOrder)
            ? await BuildSnapshotProfileLineDataForWriteAsync(currentOrder)
            : await _jigTypeResolver.ResolveProfileLineDataForWriteAsync(machineSlot, currentOrder.ModelId, currentOrder.ModelName);

        // Write order & model data registers according to D5500 - D5574.0 spec
        await _plcService.WriteAsync(PlcTagCatalog.DataAutos.OrderCountOnRotaryTable.Name, shelfOrderCount);
        await _plcService.WriteAsync(PlcTagCatalog.DataAutos.OrderCode.Name, orderCode);
        await _plcService.WriteAsync(PlcTagCatalog.DataAutos.ModelId.Name, modelId);
        await _plcService.WriteAsync(PlcTagCatalog.DataAutos.QuantityInOrder.Name, currentOrder.Quantity);
        // D5536 stores the cart/Jig position containing this order (1-4), not the Jig type.
        await _plcService.WriteAsync(PlcTagCatalog.DataAutos.JigSupplyType.Name, currentOrder.CartPositionIndex);

        // Model parameter registers D5540 - D5556 and D5560 - D5563
        await _plcService.WriteAsync(PlcTagCatalog.DataAutos.OuterFinishedDiameter.Name, profileLineData.OuterFinishedDiameter);
        await _plcService.WriteAsync(PlcTagCatalog.DataAutos.InputBlankThickness.Name, profileLineData.InputBlankThickness);
        await _plcService.WriteAsync(PlcTagCatalog.DataAutos.Op1TurnedThickness.Name, profileLineData.Op1TurnedThickness);
        await _plcService.WriteAsync(PlcTagCatalog.DataAutos.FinishedThickness.Name, profileLineData.FinishedThickness);
        await _plcService.WriteAsync(PlcTagCatalog.DataAutos.PickDropZOffset.Name, profileLineData.PickDropZOffset);
        await _plcService.WriteAsync(PlcTagCatalog.DataAutos.ChuckStepDepth.Name, profileLineData.ChuckStepDepth);
        await _plcService.WriteAsync(PlcTagCatalog.DataAutos.InnerFinishedDiameter.Name, profileLineData.InnerFinishedDiameter);
        await _plcService.WriteAsync(PlcTagCatalog.DataAutos.InnerDiameterToGDiameterDistance.Name, profileLineData.InnerDiameterToGDiameterDistance);
        await _plcService.WriteAsync(PlcTagCatalog.DataAutos.MagnetCount.Name, profileLineData.MagnetCount);
        await _plcService.WriteAsync(PlcTagCatalog.DataAutos.InputBlankDiameter.Name, profileLineData.InputBlankDiameter);
        await _plcService.WriteAsync(PlcTagCatalog.DataAutos.Op2ChuckSleeveDepth.Name, profileLineData.Op2ChuckSleeveDepth);

        await _plcService.WriteAsync(PlcTagCatalog.DataAutos.CurrentPickIndex.Name, 1);
        await _plcService.WriteAsync(PlcTagCatalog.DataAutos.OrderDataLoadCommand.Name, true);
        await _plcService.WriteAsync(PlcTagCatalog.DataAutos.ProductionResultAcknowledged.Name, false);
        await _plcService.WriteAsync(PlcTagCatalog.DataAutos.CancelOrderCommand.Name, false);
        await _plcService.WriteAsync(PlcTagCatalog.DataAutos.ShelfOrdersCompleted.Name, false);
    }

    public async Task ClearOrderAsync(AgvPosition machineSlot, bool clearCompletedBit = true)
    {
        await _plcService.WriteAsync(PlcTagCatalog.DataAutos.OrderCountOnRotaryTable.Name, 0);
        await _plcService.WriteAsync(PlcTagCatalog.DataAutos.OrderCode.Name, string.Empty);
        await _plcService.WriteAsync(PlcTagCatalog.DataAutos.ModelId.Name, string.Empty);
        await _plcService.WriteAsync(PlcTagCatalog.DataAutos.QuantityInOrder.Name, 0);
        await _plcService.WriteAsync(PlcTagCatalog.DataAutos.JigSupplyType.Name, 0);

        await _plcService.WriteAsync(PlcTagCatalog.DataAutos.OuterFinishedDiameter.Name, 0f);
        await _plcService.WriteAsync(PlcTagCatalog.DataAutos.InputBlankThickness.Name, 0f);
        await _plcService.WriteAsync(PlcTagCatalog.DataAutos.Op1TurnedThickness.Name, 0f);
        await _plcService.WriteAsync(PlcTagCatalog.DataAutos.FinishedThickness.Name, 0f);
        await _plcService.WriteAsync(PlcTagCatalog.DataAutos.PickDropZOffset.Name, 0f);
        await _plcService.WriteAsync(PlcTagCatalog.DataAutos.ChuckStepDepth.Name, 0f);
        await _plcService.WriteAsync(PlcTagCatalog.DataAutos.InnerFinishedDiameter.Name, 0f);
        await _plcService.WriteAsync(PlcTagCatalog.DataAutos.InnerDiameterToGDiameterDistance.Name, 0f);
        await _plcService.WriteAsync(PlcTagCatalog.DataAutos.MagnetCount.Name, 0);
        await _plcService.WriteAsync(PlcTagCatalog.DataAutos.InputBlankDiameter.Name, 0f);
        await _plcService.WriteAsync(PlcTagCatalog.DataAutos.Op2ChuckSleeveDepth.Name, 0f);

        await _plcService.WriteAsync(PlcTagCatalog.DataAutos.CurrentPickIndex.Name, 0);
        await _plcService.WriteAsync(PlcTagCatalog.DataAutos.OrderDataLoadCommand.Name, false);
        await _plcService.WriteAsync(PlcTagCatalog.DataAutos.ProductionResultAcknowledged.Name, false);
        if (clearCompletedBit)
        {
            await _plcService.WriteAsync(PlcTagCatalog.DataAutos.ShelfOrdersCompleted.Name, false);
        }
    }

    public Task SetProductionResultAcknowledgedAsync(AgvPosition machineSlot, bool acknowledged)
    {
        return _plcService.WriteAsync(PlcTagCatalog.DataAutos.ProductionResultAcknowledged.Name, acknowledged);
    }

    public Task SetClearRequestedAsync(AgvPosition machineSlot, bool requested)
    {
        return _plcService.WriteAsync(PlcTagCatalog.DataAutos.CancelOrderCommand.Name, requested);
    }

    public void AttachModelProfileApiClient(IModelProfileApiClient modelProfileApiClient)
    {
        _jigTypeResolver.AttachModelProfileApiClient(modelProfileApiClient);
    }

    private static bool HasProfileSnapshot(AgvOrderData order)
    {
        return order.PartHoverHeight.HasValue
               || order.JigCenterOffset.HasValue
               || order.JigDepthOffset.HasValue
               || order.DiameterOp1.HasValue
               || order.InputBlankDiameter.HasValue
               || order.Op2ChuckSleeveDepth.HasValue;
    }

    private static RobotProfileLineData BuildProfileLineDataFromSnapshot(AgvOrderData order)
    {
        return new RobotProfileLineData
        {
            JigType = order.JigType,
            PartHoverHeight = order.PartHoverHeight ?? 0f,
            JigCenterOffset = order.JigCenterOffset ?? 0f,
            JigDepthOffset = order.JigDepthOffset ?? 0f,
            DiameterOp1 = order.DiameterOp1 ?? 0f,
            InputBlankDiameter = order.InputBlankDiameter ?? 0f,
            Op2ChuckSleeveDepth = order.Op2ChuckSleeveDepth ?? 0f
        };
    }

    private async Task<RobotProfileLineData> BuildSnapshotProfileLineDataForWriteAsync(AgvOrderData order)
    {
        await _jigTypeResolver.EnsureProfileEnabledForWriteAsync(order.ModelId, order.ModelName);
        return BuildProfileLineDataFromSnapshot(order);
    }
}
