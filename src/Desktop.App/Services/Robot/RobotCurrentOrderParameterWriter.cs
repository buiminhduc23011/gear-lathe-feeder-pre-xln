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
        await _plcService.WriteAsync(PlcTagCatalog.DataAutos.JigSupplyType.Name, profileLineData.JigType);

        // Model parameter registers D5540 - D5556
        await _plcService.WriteAsync(PlcTagCatalog.DataAutos.OuterFinishedDiameter.Name, profileLineData.OuterFinishedDiameter);
        await _plcService.WriteAsync(PlcTagCatalog.DataAutos.InputBlankThickness.Name, profileLineData.InputBlankThickness);
        await _plcService.WriteAsync(PlcTagCatalog.DataAutos.Op1TurnedThickness.Name, profileLineData.Op1TurnedThickness);
        await _plcService.WriteAsync(PlcTagCatalog.DataAutos.FinishedThickness.Name, profileLineData.FinishedThickness);
        await _plcService.WriteAsync(PlcTagCatalog.DataAutos.PickDropZOffset.Name, profileLineData.PickDropZOffset);
        await _plcService.WriteAsync(PlcTagCatalog.DataAutos.ChuckStepDepth.Name, profileLineData.ChuckStepDepth);
        await _plcService.WriteAsync(PlcTagCatalog.DataAutos.InnerFinishedDiameter.Name, profileLineData.InnerFinishedDiameter);
        await _plcService.WriteAsync(PlcTagCatalog.DataAutos.InnerDiameterToGDiameterDistance.Name, profileLineData.InnerDiameterToGDiameterDistance);
        await _plcService.WriteAsync(PlcTagCatalog.DataAutos.MagnetCount.Name, profileLineData.MagnetCount);

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
        var tags = GetLineTags(machineSlot);
        return _plcService.WriteAsync(tags.ProductionResultAcknowledged.Name, acknowledged);
    }

    public Task SetClearRequestedAsync(AgvPosition machineSlot, bool requested)
    {
        return _plcService.WriteAsync(GetLineClearRequestTag(machineSlot).Name, requested);
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
               || order.DiameterOp1.HasValue;
    }

    private static RobotProfileLineData BuildProfileLineDataFromSnapshot(AgvOrderData order)
    {
        return new RobotProfileLineData
        {
            JigType = order.JigType,
            PartHoverHeight = order.PartHoverHeight ?? 0f,
            JigCenterOffset = order.JigCenterOffset ?? 0f,
            JigDepthOffset = order.JigDepthOffset ?? 0f,
            DiameterOp1 = order.DiameterOp1 ?? 0f
        };
    }

    private async Task<RobotProfileLineData> BuildSnapshotProfileLineDataForWriteAsync(AgvOrderData order)
    {
        await _jigTypeResolver.EnsureProfileEnabledForWriteAsync(order.ModelId, order.ModelName);
        return BuildProfileLineDataFromSnapshot(order);
    }

    private static LineOrderTags GetLineTags(AgvPosition position)
    {
        return position == AgvPosition.Position1
            ? new LineOrderTags(
                PlcTagCatalog.DataAutos.Shelf1ProductCount,
                PlcTagCatalog.DataAutos.Shelf1OrderCount,
                PlcTagCatalog.DataAutos.OrderLine1Code,
                PlcTagCatalog.DataAutos.OrderLine1ModelId,
                PlcTagCatalog.DataAutos.OrderLine1Quantity,
                PlcTagCatalog.DataAutos.OrderLine1JigType,
                PlcTagCatalog.DataAutos.OrderLine1StartPosition,
                PlcTagCatalog.DataAutos.OrderLine1TrayIndex,
                PlcTagCatalog.DataAutos.OrderLine1TrayType,
                PlcTagCatalog.DataAutos.OrderLine1Sequence,
                /*
                PlcTagCatalog.DataAutos.OrderLine1CheckPoint1X,
                PlcTagCatalog.DataAutos.OrderLine1CheckPoint1Y,
                PlcTagCatalog.DataAutos.OrderLine1CheckPoint1Z,
                */
                PlcTagCatalog.DataAutos.OrderLine1PartHoverHeight,
                PlcTagCatalog.DataAutos.OrderLine1JigCenterOffset,
                PlcTagCatalog.DataAutos.OrderLine1JigDepthOffset,
                PlcTagCatalog.DataAutos.OrderLine1DiameterOp1,
                PlcTagCatalog.DataAutos.OrderLine1CurrentPickIndex,
                PlcTagCatalog.DataAutos.OrderLine1IsLoading,
                PlcTagCatalog.DataAutos.OrderLine1ProductionResultAcknowledged,
                PlcTagCatalog.DataAutos.CurrentOrderLoadCompleted,
                PlcTagCatalog.DataAutos.OrderLine1ShelfOrdersCompleted)
            : new LineOrderTags(
                PlcTagCatalog.DataAutos.Shelf2ProductCount,
                PlcTagCatalog.DataAutos.Shelf2OrderCount,
                PlcTagCatalog.DataAutos.OrderLine2Code,
                PlcTagCatalog.DataAutos.OrderLine2ModelId,
                PlcTagCatalog.DataAutos.OrderLine2Quantity,
                PlcTagCatalog.DataAutos.OrderLine2JigType,
                PlcTagCatalog.DataAutos.OrderLine2StartPosition,
                PlcTagCatalog.DataAutos.OrderLine2TrayIndex,
                PlcTagCatalog.DataAutos.OrderLine2TrayType,
                PlcTagCatalog.DataAutos.OrderLine2Sequence,
                /*
                PlcTagCatalog.DataAutos.OrderLine2CheckPoint1X,
                PlcTagCatalog.DataAutos.OrderLine2CheckPoint1Y,
                PlcTagCatalog.DataAutos.OrderLine2CheckPoint1Z,
                */
                PlcTagCatalog.DataAutos.OrderLine2PartHoverHeight,
                PlcTagCatalog.DataAutos.OrderLine2JigCenterOffset,
                PlcTagCatalog.DataAutos.OrderLine2JigDepthOffset,
                PlcTagCatalog.DataAutos.OrderLine2DiameterOp1,
                PlcTagCatalog.DataAutos.OrderLine2CurrentPickIndex,
                PlcTagCatalog.DataAutos.OrderLine2IsLoading,
                PlcTagCatalog.DataAutos.OrderLine2ProductionResultAcknowledged,
                PlcTagCatalog.DataAutos.CurrentOrderLoadCompletedLine2,
                PlcTagCatalog.DataAutos.OrderLine2ShelfOrdersCompleted);
    }

    private static PlcTagDefinition GetLineClearRequestTag(AgvPosition position)
    {
        return position == AgvPosition.Position1
            ? PlcTagCatalog.DataAutos.OrderLine1ClearRequestedByPc
            : PlcTagCatalog.DataAutos.OrderLine2ClearRequestedByPc;
    }

    private sealed record LineOrderTags(
        PlcTagDefinition ShelfProductCount,
        PlcTagDefinition ShelfOrderCount,
        PlcTagDefinition OrderCode,
        PlcTagDefinition ModelId,
        PlcTagDefinition Quantity,
        PlcTagDefinition JigType,
        PlcTagDefinition StartPosition,
        PlcTagDefinition TrayIndex,
        PlcTagDefinition TrayType,
        PlcTagDefinition Sequence,
        /*
        PlcTagDefinition CheckPoint1X,
        PlcTagDefinition CheckPoint1Y,
        PlcTagDefinition CheckPoint1Z,
        */
        PlcTagDefinition PartHoverHeight,
        PlcTagDefinition JigCenterOffset,
        PlcTagDefinition JigDepthOffset,
        PlcTagDefinition DiameterOp1,
        PlcTagDefinition CurrentPickIndex,
        PlcTagDefinition IsLoading,
        PlcTagDefinition ProductionResultAcknowledged,
        PlcTagDefinition CurrentOrderLoadCompleted,
        PlcTagDefinition ShelfOrdersCompleted);
}
