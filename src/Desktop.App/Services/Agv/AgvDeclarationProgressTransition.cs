using Desktop.App.Models.Agv;
using Desktop.App.Services.Abstractions;

namespace Desktop.App.Services.Agv;

internal enum AgvDeclarationProgressTransitionResult
{
    None,
    OrderCompletionPostFailed,
    AcknowledgedCompletion,
    AdvancedToNextOrder,
    ShelfCompleted
}

internal sealed class AgvDeclarationProgressTransition
{
    private readonly IRobotCurrentOrderService _robotOrderService;

    public AgvDeclarationProgressTransition(IRobotCurrentOrderService robotOrderService)
    {
        _robotOrderService = robotOrderService;
    }

    public AgvOrderData? ResolveCurrentOrder(AgvPositionState state)
    {
        if (state.Orders.Count == 0 || state.CurrentOrderSequence <= 0)
        {
            return null;
        }

        return state.Orders
            .OrderBy(order => order.OrderSequence)
            .FirstOrDefault(order => order.OrderSequence == state.CurrentOrderSequence);
    }

    public async Task<AgvDeclarationProgressTransitionResult> HandleAsync(
        AgvPositionState state,
        bool orderCompletedFlag,
        Func<int, int, string, AgvOrderData?, Task<bool>> postMachineEventAsync,
        Func<AgvPositionState, Task> saveStateCacheAsync)
    {
        if (!state.ActiveDeclarationId.HasValue || state.Orders.Count == 0)
        {
            return AgvDeclarationProgressTransitionResult.None;
        }

        var currentOrder = ResolveCurrentOrder(state);
        if (currentOrder is null)
        {
            return AgvDeclarationProgressTransitionResult.None;
        }

        if (!state.CompletionAcknowledged)
        {
            if (!orderCompletedFlag)
            {
                return AgvDeclarationProgressTransitionResult.None;
            }

            var orderCompletedPosted = await postMachineEventAsync(
                state.ActiveDeclarationId.Value,
                (int)state.Position,
                "OrderCompleted",
                currentOrder);
            if (!orderCompletedPosted)
            {
                return AgvDeclarationProgressTransitionResult.OrderCompletionPostFailed;
            }

            state.CompletionAcknowledged = true;
            await saveStateCacheAsync(state);
            await _robotOrderService.SetProductionResultAcknowledgedAsync(state.Position, true);
            return AgvDeclarationProgressTransitionResult.AcknowledgedCompletion;
        }

        if (orderCompletedFlag)
        {
            return AgvDeclarationProgressTransitionResult.None;
        }

        var nextOrder = state.Orders
            .OrderBy(order => order.OrderSequence)
            .FirstOrDefault(order => order.OrderSequence > currentOrder.OrderSequence);

        if (nextOrder is not null)
        {
            var shelfProductCount = state.Orders.Sum(order => Math.Max(0, order.Quantity));
            await _robotOrderService.LoadCurrentOrderAsync(state.Position, shelfProductCount, state.Orders.Count, state.ShelfLayoutType, nextOrder);
            state.CurrentOrderSequence = nextOrder.OrderSequence;
            state.CurrentItemInOrder = 0;
            state.AwaitingPickReset = false;
            state.CompletionAcknowledged = false;
            await saveStateCacheAsync(state);
            return AgvDeclarationProgressTransitionResult.AdvancedToNextOrder;
        }

        if (state.CompletionReported)
        {
            return AgvDeclarationProgressTransitionResult.None;
        }

        state.CompletionReported = true;
        state.CompletionAcknowledged = true;
        state.CurrentOrderSequence = state.Orders.Count + 1;
        state.CurrentModelName = "—";
        await saveStateCacheAsync(state);
        return AgvDeclarationProgressTransitionResult.ShelfCompleted;
    }
}
