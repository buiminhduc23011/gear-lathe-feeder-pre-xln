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

        var tags = GetLineTags(machineSlot);
        var trayTypeForPlc = _trayPlacementResolver.ResolveTrayType(shelfLayoutType, currentOrder.TrayIndex);
        var startPositionForPlc = await _trayPlacementResolver.ResolveStartPositionAsync(currentOrder.StartPosition, trayTypeForPlc);
        var profileLineData = HasProfileSnapshot(currentOrder)
            ? await BuildSnapshotProfileLineDataForWriteAsync(currentOrder)
            : await _jigTypeResolver.ResolveProfileLineDataForWriteAsync(machineSlot, currentOrder.ModelId, currentOrder.ModelName);
        var jigTypeForPlc = profileLineData.JigType;

        await _plcService.WriteAsync(tags.ShelfProductCount.Name, shelfProductCount);
        await _plcService.WriteAsync(tags.ShelfOrderCount.Name, shelfOrderCount);
        await _plcService.WriteAsync(tags.OrderCode.Name, orderCode);
        await _plcService.WriteAsync(tags.ModelId.Name, modelId);
        await _plcService.WriteAsync(tags.Quantity.Name, currentOrder.Quantity);
        await _plcService.WriteAsync(tags.JigType.Name, jigTypeForPlc);
        await _plcService.WriteAsync(tags.StartPosition.Name, startPositionForPlc);
        await _plcService.WriteAsync(tags.TrayIndex.Name, currentOrder.TrayIndex);
        await _plcService.WriteAsync(tags.TrayType.Name, trayTypeForPlc);
        await _plcService.WriteAsync(tags.Sequence.Name, currentOrder.OrderSequence);
        // Cải tiến theo yêu cầu mr.Tùng ngày 27/04/2026: Ngừng ghi các điểm check gốc robot
        /*
        await _plcService.WriteAsync(tags.CheckPoint1X.Name, profileLineData.CheckPoint1X);
        await _plcService.WriteAsync(tags.CheckPoint1Y.Name, profileLineData.CheckPoint1Y);
        await _plcService.WriteAsync(tags.CheckPoint1Z.Name, profileLineData.CheckPoint1Z);
        */
        await _plcService.WriteAsync(tags.PartHoverHeight.Name, profileLineData.PartHoverHeight);
        await _plcService.WriteAsync(tags.JigCenterOffset.Name, profileLineData.JigCenterOffset);
        await _plcService.WriteAsync(tags.JigDepthOffset.Name, profileLineData.JigDepthOffset);
        await _plcService.WriteAsync(tags.DiameterOp1.Name, profileLineData.DiameterOp1);
        // await _plcService.WriteAsync(tags.CurrentPickIndex.Name, currentOrder.OrderSequence);
        await _plcService.WriteAsync(tags.IsLoading.Name, false);
        await _plcService.WriteAsync(tags.ProductionResultAcknowledged.Name, false);
        await _plcService.WriteAsync(GetLineClearRequestTag(machineSlot).Name, false);
        await _plcService.WriteAsync(tags.ShelfOrdersCompleted.Name, false);
        await _plcService.WriteAsync(tags.CurrentOrderLoadCompleted.Name, true);
    }

    public async Task ClearOrderAsync(AgvPosition machineSlot, bool clearCompletedBit = true)
    {
        var tags = GetLineTags(machineSlot);

        await _plcService.WriteAsync(tags.ShelfProductCount.Name, 0);
        await _plcService.WriteAsync(tags.ShelfOrderCount.Name, 0);
        await _plcService.WriteAsync(tags.OrderCode.Name, string.Empty);
        await _plcService.WriteAsync(tags.ModelId.Name, string.Empty);
        await _plcService.WriteAsync(tags.Quantity.Name, 0);
        await _plcService.WriteAsync(tags.JigType.Name, 0);
        await _plcService.WriteAsync(tags.StartPosition.Name, 0);
        await _plcService.WriteAsync(tags.TrayIndex.Name, 0);
        await _plcService.WriteAsync(tags.TrayType.Name, 0);
        await _plcService.WriteAsync(tags.Sequence.Name, 0);
        // Cải tiến theo yêu cầu mr.Tùng ngày 27/04/2026: Ngừng ghi các điểm check gốc robot
        /*
        await _plcService.WriteAsync(tags.CheckPoint1X.Name, 0f);
        await _plcService.WriteAsync(tags.CheckPoint1Y.Name, 0f);
        await _plcService.WriteAsync(tags.CheckPoint1Z.Name, 0f);
        */
        await _plcService.WriteAsync(tags.PartHoverHeight.Name, 0f);
        await _plcService.WriteAsync(tags.JigCenterOffset.Name, 0f);
        await _plcService.WriteAsync(tags.JigDepthOffset.Name, 0f);
        await _plcService.WriteAsync(tags.DiameterOp1.Name, 0f);
        // await _plcService.WriteAsync(tags.CurrentPickIndex.Name, 0);
        await _plcService.WriteAsync(tags.IsLoading.Name, false);
        await _plcService.WriteAsync(tags.ProductionResultAcknowledged.Name, false);
        await _plcService.WriteAsync(tags.CurrentOrderLoadCompleted.Name, false);
        if (clearCompletedBit)
        {
            await _plcService.WriteAsync(tags.ShelfOrdersCompleted.Name, false);
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
