using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Server.Api.Contracts.Requests;
using Server.Api.Contracts.Responses;
using Server.Api.Data;
using Server.Api.Data.Entities;
using Server.Api.Infrastructure;

namespace Server.Api.Services;

public sealed class ShelfDeclarationService : IShelfDeclarationService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly AppDbContext _db;

    private static readonly Dictionary<int, (int Tray1, int Tray2)> LayoutTrayTypes = new()
    {
        [1] = (1, 1),
        [2] = (2, 2),
        [3] = (1, 2),
        [4] = (2, 1),
    };

    public ShelfDeclarationService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ManualShelfDeclarationEntity> CreateAsync(
        int machineId,
        CreateShelfDeclarationRequest request,
        string username,
        CancellationToken cancellationToken = default)
    {
        var normalizedMode = NormalizeMode(request.Mode);
        ValidateCreateRequest(request, normalizedMode);

        var machine = await _db.Machines
            .FirstOrDefaultAsync(m => m.MachineId == machineId, cancellationToken)
            ?? throw new InvalidOperationException($"Machine {machineId} was not found.");

        await EnsureSlotAvailableAsync(machine, normalizedMode, request.StagingSlotIndex, request.MachineSlotIndex, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var ordersJson = await BuildOrdersJsonAsync(machineId, machine, request, normalizedMode, cancellationToken);

        var entity = new ManualShelfDeclarationEntity
        {
            MachineId = machineId,
            Mode = normalizedMode,
            StagingSlotIndex = normalizedMode == ShelfDeclarationModes.Agv ? request.StagingSlotIndex : null,
            MachineSlotIndex = normalizedMode == ShelfDeclarationModes.ManualLoad ? request.MachineSlotIndex : null,
            ShelfLayoutType = request.ShelfLayoutType,
            OrdersJson = ordersJson,
            Status = ShelfDeclarationStatuses.Created,
            MachineCodeSnapshot = machine.MachineCode,
            MachineNameSnapshot = machine.MachineName,
            CreatedByUsername = username,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        _db.ManualShelfDeclarations.Add(entity);
        _db.ShelfDeclarationEvents.Add(CreateEvent(
            entity,
            ShelfDeclarationEventTypes.Created,
            ShelfDeclarationActorTypes.User,
            actorName: username,
            eventAtUtc: now,
            payload: new { entity.Mode, entity.StagingSlotIndex, entity.MachineSlotIndex, entity.ShelfLayoutType }));

        await _db.SaveChangesAsync(cancellationToken);

        entity.Machine = machine;
        return entity;
    }

    public async Task<List<ManualShelfDeclarationEntity>> GetByMachineAsync(
        int machineId,
        CancellationToken cancellationToken = default)
    {
        return await _db.ManualShelfDeclarations
            .Include(d => d.Machine)
            .Where(d => d.MachineId == machineId)
            .OrderByDescending(d => d.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ShelfDeclarationSlotStatusResponse>> GetSlotStatusesAsync(
        int machineId,
        CancellationToken cancellationToken = default)
    {
        var snapshots = await GetActiveAgvSlotSnapshotsAsync(cancellationToken);

        return snapshots
            .Select(snapshot => new ShelfDeclarationSlotStatusResponse
            {
                SlotIndex = snapshot.SlotIndex,
                IsOccupied = snapshot.MachineId == machineId && snapshot.IsOccupied,
                DeclarationId = snapshot.MachineId == machineId ? snapshot.DeclarationId : null,
                Status = snapshot.MachineId == machineId ? snapshot.Status : null,
                CreatedByUsername = snapshot.MachineId == machineId ? snapshot.CreatedByUsername : null,
                CreatedAtUtc = snapshot.MachineId == machineId ? snapshot.CreatedAtUtc : null,
                ShelfLayoutType = snapshot.MachineId == machineId ? snapshot.ShelfLayoutType : null,
                ShelfLayoutName = snapshot.MachineId == machineId ? snapshot.ShelfLayoutName : null,
                OrderCount = snapshot.MachineId == machineId ? snapshot.OrderCount : null
            })
            .ToList();
    }

    public async Task<List<AgvSlotStatusResponse>> GetAgvSlotStatusesAsync(
        CancellationToken cancellationToken = default)
    {
        var snapshots = await GetActiveAgvSlotSnapshotsAsync(cancellationToken);

        return snapshots
            .Select(snapshot => new AgvSlotStatusResponse
            {
                SlotIndex = snapshot.SlotIndex,
                IsOccupied = snapshot.IsOccupied,
                DeclarationId = snapshot.DeclarationId,
                MachineId = snapshot.MachineId,
                MachineCode = snapshot.MachineCode,
                MachineName = snapshot.MachineName,
                Status = snapshot.Status,
                ShelfLayoutType = snapshot.ShelfLayoutType,
                ShelfLayoutName = snapshot.ShelfLayoutName,
                OrderCount = snapshot.OrderCount,
                CreatedAtUtc = snapshot.CreatedAtUtc
            })
            .ToList();
    }

    public async Task<AgvCallEligibilityResponse> GetAgvCallEligibilityAsync(
        string machineCode,
        int machineSlotIndex,
        CancellationToken cancellationToken = default)
    {
        var normalizedMachineCode = machineCode.Trim();
        if (string.IsNullOrWhiteSpace(normalizedMachineCode))
        {
            throw new InvalidOperationException("machineCode is required.");
        }

        if (machineSlotIndex is < 1 or > 2)
        {
            throw new InvalidOperationException("machineSlotIndex must be between 1 and 2.");
        }

        var machine = await _db.Machines
            .FirstOrDefaultAsync(m => m.MachineCode == normalizedMachineCode, cancellationToken);
        if (machine is null)
        {
            return new AgvCallEligibilityResponse
            {
                MachineCode = normalizedMachineCode,
                MachineSlotIndex = machineSlotIndex,
                HasActiveDeclaration = false,
                ReasonCode = "MachineNotFound",
                ReasonMessage = $"Machine '{normalizedMachineCode}' was not found."
            };
        }

        var stagingSlotIndex = GetStagingSlotForLocalMachineSlot(machine, machineSlotIndex);

        // Check AGV-mode declarations first (matched by staging slot)
        var entity = await _db.ManualShelfDeclarations
            .Where(d => d.MachineId == machine.MachineId
                        && d.Mode == ShelfDeclarationModes.Agv
                        && d.StagingSlotIndex == stagingSlotIndex
                        && d.Status == ShelfDeclarationStatuses.Created)
            .OrderBy(d => d.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        // Fallback: check ManualLoad-mode declarations (matched by machine slot directly)
        entity ??= await _db.ManualShelfDeclarations
            .Where(d => d.MachineId == machine.MachineId
                        && d.Mode == ShelfDeclarationModes.ManualLoad
                        && d.MachineSlotIndex == machineSlotIndex
                        && d.Status == ShelfDeclarationStatuses.Created)
            .OrderBy(d => d.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity is null)
        {
            return new AgvCallEligibilityResponse
            {
                MachineId = machine.MachineId,
                MachineCode = machine.MachineCode,
                MachineName = machine.MachineName,
                MachineSlotIndex = machineSlotIndex,
                StagingSlotIndex = stagingSlotIndex,
                HasActiveDeclaration = false,
                ReasonCode = "NoDeclaration",
                ReasonMessage = $"No active declaration found for machine slot {machineSlotIndex} (staging slot {stagingSlotIndex})."
            };
        }

        var orderCount = CountOrders(entity.OrdersJson);
        if (orderCount <= 0)
        {
            return new AgvCallEligibilityResponse
            {
                MachineId = machine.MachineId,
                MachineCode = machine.MachineCode,
                MachineName = machine.MachineName,
                MachineSlotIndex = machineSlotIndex,
                StagingSlotIndex = stagingSlotIndex,
                HasActiveDeclaration = false,
                DeclarationId = entity.Id,
                DeclarationStatus = entity.Status,
                OrderCount = orderCount,
                ReasonCode = "NoDeclaredOrders",
                ReasonMessage = $"Declaration #{entity.Id} has no declared orders for machine slot {machineSlotIndex} (staging slot {stagingSlotIndex})."
            };
        }

        return new AgvCallEligibilityResponse
        {
            MachineId = machine.MachineId,
            MachineCode = machine.MachineCode,
            MachineName = machine.MachineName,
            MachineSlotIndex = machineSlotIndex,
            StagingSlotIndex = stagingSlotIndex,
            HasActiveDeclaration = true,
            DeclarationId = entity.Id,
            DeclarationStatus = entity.Status,
            OrderCount = orderCount,
            ReasonCode = "Eligible",
            ReasonMessage = "Eligible for AGV call."
        };
    }

    public async Task<AgvPickupResponse?> PickupForAgvAsync(
        AgvPickupRequest request,
        CancellationToken cancellationToken = default)
    {
        var machineCode = request.MachineCode.Trim();
        var agvId = request.AgvId.Trim();
        var agvName = request.AgvName.Trim();
        if (string.IsNullOrWhiteSpace(machineCode) || string.IsNullOrWhiteSpace(agvId) || string.IsNullOrWhiteSpace(agvName))
        {
            throw new InvalidOperationException("machineCode, agvId and agvName are required.");
        }

        var machine = await _db.Machines
            .FirstOrDefaultAsync(m => m.MachineCode == machineCode, cancellationToken)
            ?? throw new InvalidOperationException($"Machine '{machineCode}' was not found.");

        var stagingSlotIndex = ValidateAgvPickupSlotIndexes(machine, request.MachineSlotIndex, request.SlotIndex);
        var entity = await _db.ManualShelfDeclarations
            .Include(d => d.Machine)
            .FirstOrDefaultAsync(d => d.MachineId == machine.MachineId
                                   && d.Mode == ShelfDeclarationModes.Agv
                                   && d.StagingSlotIndex == stagingSlotIndex
                                   && d.Status == ShelfDeclarationStatuses.Created, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        entity.Status = ShelfDeclarationStatuses.AgvTaken;
        entity.PickedByAgvId = agvId;
        entity.PickedByAgvName = agvName;
        entity.AgvTakenAtUtc = now;
        entity.UpdatedAtUtc = now;

        _db.ShelfDeclarationEvents.Add(CreateEvent(
            entity,
            ShelfDeclarationEventTypes.AgvPicked,
            ShelfDeclarationActorTypes.Agv,
            actorId: agvId,
            actorName: agvName,
            eventAtUtc: now));

        await _db.SaveChangesAsync(cancellationToken);

        return new AgvPickupResponse
        {
            DeclarationId = entity.Id
        };
    }

    public async Task<MachineLoadResponse?> GetMachineLoadAsync(
        int id,
        string machineCode,
        CancellationToken cancellationToken = default)
    {
        var normalizedMachineCode = machineCode.Trim();
        if (string.IsNullOrWhiteSpace(normalizedMachineCode))
        {
            throw new InvalidOperationException("machineCode is required.");
        }

        var entity = await _db.ManualShelfDeclarations
            .Include(d => d.Machine)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        var targetMachineCode = entity?.Machine?.MachineCode ?? entity?.MachineCodeSnapshot;
        if (entity is null || !string.Equals(targetMachineCode, normalizedMachineCode, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var machineSlotIndex = entity.Mode == ShelfDeclarationModes.Agv
            ? GetLocalMachineSlotIndex(entity.Machine ?? throw new InvalidOperationException("Machine was not loaded."), entity.StagingSlotIndex)
            : entity.MachineSlotIndex;
        if (!machineSlotIndex.HasValue)
        {
            throw new InvalidOperationException($"Declaration {id} does not have a usable machine slot.");
        }

        return new MachineLoadResponse
        {
            DeclarationId = entity.Id,
            MachineSlotIndex = machineSlotIndex.Value,
            ShelfLayoutType = entity.ShelfLayoutType,
            OrdersJson = entity.OrdersJson,
            OrderCount = CountOrders(entity.OrdersJson)
        };
    }

    public async Task<MachineLoadResponse?> GetPendingManualLoadAsync(
        string machineCode,
        int machineSlotIndex,
        CancellationToken cancellationToken = default)
    {
        var normalizedMachineCode = machineCode.Trim();
        if (string.IsNullOrWhiteSpace(normalizedMachineCode))
        {
            throw new InvalidOperationException("machineCode is required.");
        }

        if (machineSlotIndex is < 1 or > 2)
        {
            throw new InvalidOperationException("machineSlotIndex must be between 1 and 2.");
        }

        var entity = await _db.ManualShelfDeclarations
            .Where(d => d.Mode == ShelfDeclarationModes.ManualLoad
                        && d.Status == ShelfDeclarationStatuses.Created
                        && (d.MachineCodeSnapshot == normalizedMachineCode || d.Machine.MachineCode == normalizedMachineCode)
                        && d.MachineSlotIndex == machineSlotIndex)
            .OrderBy(d => d.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity is null)
        {
            return null;
        }

        return new MachineLoadResponse
        {
            DeclarationId = entity.Id,
            MachineSlotIndex = machineSlotIndex,
            ShelfLayoutType = entity.ShelfLayoutType,
            OrdersJson = entity.OrdersJson,
            OrderCount = CountOrders(entity.OrdersJson)
        };
    }

    public async Task<MachineLoadResponse?> GetActiveLoadAsync(
        string machineCode,
        int machineSlotIndex,
        CancellationToken cancellationToken = default)
    {
        var normalizedMachineCode = machineCode.Trim();
        if (string.IsNullOrWhiteSpace(normalizedMachineCode))
        {
            throw new InvalidOperationException("machineCode is required.");
        }

        if (machineSlotIndex is < 1 or > 2)
        {
            throw new InvalidOperationException("machineSlotIndex must be between 1 and 2.");
        }

        var machine = await _db.Machines
            .FirstOrDefaultAsync(m => m.MachineCode == normalizedMachineCode, cancellationToken);
        if (machine is null)
        {
            return null;
        }

        var stagingSlotIndex = GetStagingSlotForLocalMachineSlot(machine, machineSlotIndex);

        var entity = await _db.ManualShelfDeclarations
            .Where(d => d.MachineId == machine.MachineId
                        && (
                            (d.Mode == ShelfDeclarationModes.ManualLoad && d.MachineSlotIndex == machineSlotIndex)
                            || (d.Mode == ShelfDeclarationModes.Agv && d.StagingSlotIndex == stagingSlotIndex)
                        )
                        && d.Status == ShelfDeclarationStatuses.InProduction)
            .OrderByDescending(d => d.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity is null)
        {
            return null;
        }

        return new MachineLoadResponse
        {
            DeclarationId = entity.Id,
            MachineSlotIndex = machineSlotIndex,
            ShelfLayoutType = entity.ShelfLayoutType,
            OrdersJson = entity.OrdersJson,
            OrderCount = CountOrders(entity.OrdersJson)
        };
    }

    public async Task<bool> RequestLoadAsync(int id, string username, CancellationToken cancellationToken = default)
    {
        var entity = await _db.ManualShelfDeclarations
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (entity is null
            || entity.Mode != ShelfDeclarationModes.ManualLoad
            || entity.Status != ShelfDeclarationStatuses.Created)
        {
            return false;
        }

        var now = DateTimeOffset.UtcNow;
        entity.LoadRequestedAtUtc ??= now;
        entity.UpdatedAtUtc = now;

        _db.ShelfDeclarationEvents.Add(CreateEvent(
            entity,
            ShelfDeclarationEventTypes.LoadRequested,
            ShelfDeclarationActorTypes.User,
            actorName: username,
            eventAtUtc: now));

        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<List<ManualShelfDeclarationEntity>> GetLoadRequestsByMachineCodeAsync(
        string machineCode,
        CancellationToken cancellationToken = default)
    {
        return await _db.ManualShelfDeclarations
            .Include(d => d.Machine)
            .Where(d => d.Machine.MachineCode == machineCode
                        && d.Mode == ShelfDeclarationModes.ManualLoad
                        && d.Status == ShelfDeclarationStatuses.Created
                        && d.LoadRequestedAtUtc != null)
            .OrderBy(d => d.LoadRequestedAtUtc)
            .ThenBy(d => d.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<ManualShelfDeclarationEntity?> ApplyMachineEventAsync(
        int id,
        MachineShelfEventRequest request,
        CancellationToken cancellationToken = default)
    {
        var eventType = request.EventType?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(request.MachineCode))
        {
            throw new InvalidOperationException("machineCode is required.");
        }

        var entity = await _db.ManualShelfDeclarations
            .Include(d => d.Machine)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        var targetMachineCode = entity?.Machine?.MachineCode ?? entity?.MachineCodeSnapshot;
        if (entity is null || !string.Equals(targetMachineCode, request.MachineCode.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        entity.MachineSlotIndex = request.MachineSlotIndex;
        object? eventPayload = null;

        switch (eventType)
        {
            case ShelfDeclarationEventTypes.Loaded:
                if (entity.Status != ShelfDeclarationStatuses.Created
                    && entity.Status != ShelfDeclarationStatuses.AgvTaken)
                {
                    return null;
                }

                entity.Status = ShelfDeclarationStatuses.Loaded;
                entity.LoadedAtUtc ??= now;
                break;

            case ShelfDeclarationEventTypes.InProduction:
                if (entity.Status != ShelfDeclarationStatuses.Loaded
                    && entity.Status != ShelfDeclarationStatuses.AgvTaken)
                {
                    return null;
                }

                entity.LoadedAtUtc ??= now;
                entity.ProductionStartedAtUtc ??= now;
                entity.Status = ShelfDeclarationStatuses.InProduction;
                break;

            case ShelfDeclarationEventTypes.OrderCompleted:
                eventPayload = ApplyOrderCompletedEvent(entity, request, now);
                break;

            case ShelfDeclarationEventTypes.Completed:
                if (entity.Status != ShelfDeclarationStatuses.InProduction)
                {
                    return null;
                }

                entity.CompletedAtUtc = now;
                entity.Status = ShelfDeclarationStatuses.Completed;
                entity.ProductionDurationSeconds = ComputeDurationSeconds(entity.ProductionStartedAtUtc, entity.CompletedAtUtc);
                break;

            case ShelfDeclarationEventTypes.Cleared:
                if (entity.Status == ShelfDeclarationStatuses.Cancelled)
                {
                    return null;
                }

                entity.ClearedAtUtc = now;
                entity.ClearedByUsername = !string.IsNullOrWhiteSpace(request.ActorUsername)
                    ? request.ActorUsername.Trim()
                    : request.MachineCode.Trim();
                entity.Status = ShelfDeclarationStatuses.Cleared;
                entity.ProductionDurationSeconds ??= ComputeDurationSeconds(entity.ProductionStartedAtUtc, entity.ClearedAtUtc);
                break;

            default:
                throw new InvalidOperationException($"Unsupported machine event '{eventType}'.");
        }

        entity.UpdatedAtUtc = now;
        _db.ShelfDeclarationEvents.Add(CreateEvent(
            entity,
            eventType,
            ShelfDeclarationActorTypes.Desktop,
            actorName: request.MachineCode.Trim(),
            eventAtUtc: now,
            payload: eventPayload));

        await _db.SaveChangesAsync(cancellationToken);
        return entity;
    }

    private static object ApplyOrderCompletedEvent(
        ManualShelfDeclarationEntity entity,
        MachineShelfEventRequest request,
        DateTimeOffset now)
    {
        if (entity.Status != ShelfDeclarationStatuses.InProduction
            && entity.Status != ShelfDeclarationStatuses.Completed)
        {
            throw new InvalidOperationException("OrderCompleted requires declaration status InProduction.");
        }

        if (!request.OrderSequence.HasValue || request.OrderSequence.Value <= 0)
        {
            throw new InvalidOperationException("orderSequence is required for OrderCompleted.");
        }

        var orders = ParseOrders(entity.OrdersJson).ToList();
        var order = orders.FirstOrDefault(o => o.OrderSequence == request.OrderSequence.Value);
        if (order is null)
        {
            throw new InvalidOperationException($"Order sequence {request.OrderSequence.Value} was not found.");
        }

        var requestOrderId = request.OrderId?.Trim();
        if (!string.IsNullOrWhiteSpace(requestOrderId)
            && !string.Equals(order.OrderId?.Trim(), requestOrderId, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"orderId '{requestOrderId}' does not match order sequence {request.OrderSequence.Value}.");
        }

        var previousIncompleteOrder = orders
            .Where(o => o.OrderSequence < order.OrderSequence)
            .OrderBy(o => o.OrderSequence)
            .FirstOrDefault(o => !string.Equals(o.Status, ShelfDeclarationStatuses.Completed, StringComparison.OrdinalIgnoreCase));
        if (previousIncompleteOrder is not null)
        {
            throw new InvalidOperationException($"Order sequence {previousIncompleteOrder.OrderSequence} must be completed first.");
        }

        var wasAlreadyCompleted = string.Equals(order.Status, ShelfDeclarationStatuses.Completed, StringComparison.OrdinalIgnoreCase);
        if (!wasAlreadyCompleted)
        {
            order.Status = ShelfDeclarationStatuses.Completed;
            order.CompletedAtUtc = now;
            entity.OrdersJson = JsonSerializer.Serialize(orders, JsonOptions);
        }

        if (orders.Count > 0
            && orders.All(o => string.Equals(o.Status, ShelfDeclarationStatuses.Completed, StringComparison.OrdinalIgnoreCase)))
        {
            entity.CompletedAtUtc ??= now;
            entity.Status = ShelfDeclarationStatuses.Completed;
            entity.ProductionDurationSeconds = ComputeDurationSeconds(entity.ProductionStartedAtUtc, entity.CompletedAtUtc);
        }

        return new
        {
            request.OrderSequence,
            OrderId = order.OrderId,
            AlreadyCompleted = wasAlreadyCompleted
        };
    }

    public async Task<List<ShelfDeclarationEventEntity>> GetEventsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _db.ShelfDeclarationEvents
            .Where(e => e.DeclarationId == id)
            .OrderBy(e => e.EventAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<ProductionLifecycleReportResponse> GetProductionLifecycleReportAsync(
        int? machineId,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        string? status,
        int? shelfIndex,
        CancellationToken cancellationToken = default)
    {
        if (shelfIndex.HasValue && shelfIndex is (< 1 or > 2))
        {
            throw new InvalidOperationException("shelfIndex must be between 1 and 2.");
        }

        var normalizedStatus = string.IsNullOrWhiteSpace(status) ? null : status.Trim();

        var query = _db.ManualShelfDeclarations
            .Include(d => d.Machine)
            .AsQueryable();

        if (machineId.HasValue)
        {
            query = query.Where(d => d.MachineId == machineId.Value);
        }

        if (fromUtc.HasValue)
        {
            query = query.Where(d => d.CreatedAtUtc >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            query = query.Where(d => d.CreatedAtUtc <= toUtc.Value);
        }

        if (normalizedStatus is not null)
        {
            query = query.Where(d => d.Status == normalizedStatus);
        }

        var declarations = await query
            .OrderByDescending(d => d.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var rows = declarations
            .Select(d => BuildLifecycleRow(d, ResolveShelfIndex(d)))
            .ToList();

        if (shelfIndex.HasValue)
        {
            rows = rows
                .Where(x => x.ShelfIndex == shelfIndex.Value)
                .ToList();
        }

        return new ProductionLifecycleReportResponse
        {
            GeneratedAtUtc = DateTimeOffset.UtcNow,
            MachineId = machineId,
            FromUtc = fromUtc,
            ToUtc = toUtc,
            Status = normalizedStatus,
            ShelfIndex = shelfIndex,
            TotalDeclarations = rows.Count,
            InProgressDeclarations = rows.Count(x => IsInProgressStatus(x.Status)),
            CompletedDeclarations = rows.Count(x => string.Equals(x.Status, ShelfDeclarationStatuses.Completed, StringComparison.OrdinalIgnoreCase)
                                                 || string.Equals(x.Status, ShelfDeclarationStatuses.Cleared, StringComparison.OrdinalIgnoreCase)),
            FailedDeclarations = rows.Count(x => string.Equals(x.Status, ShelfDeclarationStatuses.Cancelled, StringComparison.OrdinalIgnoreCase)),
            Items = rows
        };
    }

    public async Task<bool> CancelAsync(int id, string username, CancellationToken cancellationToken = default)
    {
        var entity = await _db.ManualShelfDeclarations.FindAsync([id], cancellationToken);
        if (entity is null
            || entity.Status != ShelfDeclarationStatuses.Created
            || !string.IsNullOrWhiteSpace(entity.PickedByAgvId)
            || !string.IsNullOrWhiteSpace(entity.PickedByAgvName)
            || entity.AgvTakenAtUtc.HasValue)
        {
            return false;
        }

        var now = DateTimeOffset.UtcNow;
        entity.Status = ShelfDeclarationStatuses.Cancelled;
        entity.CancelledAtUtc = now;
        entity.CancelledByUsername = username;
        entity.UpdatedAtUtc = now;

        _db.ShelfDeclarationEvents.Add(CreateEvent(
            entity,
            ShelfDeclarationEventTypes.Cancelled,
            ShelfDeclarationActorTypes.User,
            actorName: username,
            eventAtUtc: now));

        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task EnsureSlotAvailableAsync(
        MachineEntity machine,
        string mode,
        int? stagingSlotIndex,
        int? machineSlotIndex,
        CancellationToken cancellationToken)
    {
        if (mode == ShelfDeclarationModes.Agv)
        {
            var ownedSlots = GetAssignedStagingSlots(machine);
            if (!stagingSlotIndex.HasValue || !ownedSlots.Contains(stagingSlotIndex.Value))
            {
                throw new InvalidOperationException($"Staging slot {stagingSlotIndex} does not belong to machine {machine.MachineCode}.");
            }

            var exists = await _db.ManualShelfDeclarations.AnyAsync(
                d => d.MachineId == machine.MachineId
                     && d.Mode == ShelfDeclarationModes.Agv
                     && d.StagingSlotIndex == stagingSlotIndex
                     && d.Status == ShelfDeclarationStatuses.Created,
                cancellationToken);
            if (exists)
            {
                throw new InvalidOperationException($"Staging slot {stagingSlotIndex} already has an active declaration.");
            }

            return;
        }

        var active = await _db.ManualShelfDeclarations.AnyAsync(
            d => d.MachineId == machine.MachineId
                 && d.Mode == ShelfDeclarationModes.ManualLoad
                 && d.MachineSlotIndex == machineSlotIndex
                 && d.Status != ShelfDeclarationStatuses.Completed
                 && d.Status != ShelfDeclarationStatuses.Cleared
                 && d.Status != ShelfDeclarationStatuses.Cancelled,
            cancellationToken);
        if (active)
        {
            throw new InvalidOperationException($"Machine slot {machineSlotIndex} already has an active declaration.");
        }
    }

    private static string NormalizeMode(string mode)
    {
        if (string.Equals(mode?.Trim(), ShelfDeclarationModes.Agv, StringComparison.OrdinalIgnoreCase))
        {
            return ShelfDeclarationModes.Agv;
        }

        if (string.Equals(mode?.Trim(), ShelfDeclarationModes.ManualLoad, StringComparison.OrdinalIgnoreCase))
        {
            return ShelfDeclarationModes.ManualLoad;
        }

        throw new InvalidOperationException($"Unsupported mode '{mode}'.");
    }

    private static void ValidateCreateRequest(CreateShelfDeclarationRequest request, string normalizedMode)
    {
        ValidateOrderIds(request.Orders);

        if (normalizedMode == ShelfDeclarationModes.Agv)
        {
            if (request.StagingSlotIndex is < 1 or > 4)
            {
                throw new InvalidOperationException("stagingSlotIndex is required for Agv mode.");
            }

            return;
        }

        if (request.MachineSlotIndex is < 1 or > 2)
        {
            throw new InvalidOperationException("machineSlotIndex is required for ManualLoad mode.");
        }
    }

    private static void ValidateOrderIds(IReadOnlyList<ShelfOrderItem> orders)
    {
        for (var index = 0; index < orders.Count; index += 1)
        {
            if (string.IsNullOrWhiteSpace(orders[index].OrderId))
            {
                throw new InvalidOperationException($"Order #{index + 1} is required.");
            }
        }
    }

    private async Task<string> BuildOrdersJsonAsync(
        int machineId,
        MachineEntity machine,
        CreateShelfDeclarationRequest request,
        string normalizedMode,
        CancellationToken cancellationToken)
    {
        var trayTypes = LayoutTrayTypes.GetValueOrDefault(request.ShelfLayoutType, (Tray1: 1, Tray2: 1));
        var localMachineSlotIndex = normalizedMode == ShelfDeclarationModes.Agv
            ? GetLocalMachineSlotIndex(machine, request.StagingSlotIndex)
            : request.MachineSlotIndex;

        var profileDataByModelName = await LoadProfileDataByModelNameAsync(
            machineId,
            localMachineSlotIndex ?? 1,
            request.Orders,
            cancellationToken);

        var orders = request.Orders.Select((o, index) =>
        {
            var profileData = ResolveProfileData(o.ModelName, profileDataByModelName);
            return new
            {
                orderId = o.OrderId!.Trim(),
                modelId = profileData?.ProfileId.ToString(),
                modelName = o.ModelName,
                reportModelName = NormalizeOptionalReportModelName(o.ReportModelName),
                quantity = o.Quantity,
                startPosition = o.StartPosition,
                trayIndex = o.TrayIndex,
                trayType = o.TrayIndex == 2 ? trayTypes.Tray2 : trayTypes.Tray1,
                orderSequence = index + 1,
                jigType = profileData?.JigType ?? 0,
                partHoverHeight = profileData?.PartHoverHeight,
                jigCenterOffset = profileData?.JigCenterOffset,
                jigDepthOffset = profileData?.JigDepthOffset,
                diameterOp1 = profileData?.DiameterOp1,
                status = (string?)null,
                completedAtUtc = (DateTimeOffset?)null
            };
        });

        return JsonSerializer.Serialize(orders, JsonOptions);
    }

    private sealed record ModelProfileData(
        int ProfileId,
        int JigType,
        float PartHoverHeight,
        float JigCenterOffset,
        float JigDepthOffset,
        float? DiameterOp1);

    private async Task<Dictionary<string, ModelProfileData>> LoadProfileDataByModelNameAsync(
        int machineId,
        int localMachineSlotIndex,
        IReadOnlyList<ShelfOrderItem> orders,
        CancellationToken cancellationToken)
    {
        var requestedModelNames = orders
            .Select(o => o.ModelName?.Trim())
            .OfType<string>()
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (requestedModelNames.Count == 0)
        {
            return new Dictionary<string, ModelProfileData>(StringComparer.OrdinalIgnoreCase);
        }

        var profiles = await _db.ModelProfiles
            .AsNoTracking()
            .Where(x => x.MachineId == machineId && !x.IsDeleted)
            .Select(x => new { x.Id, x.ModelName, x.RobotData, x.Line1Data, x.Line2Data, x.DiameterOp1 })
            .ToListAsync(cancellationToken);

        var missingRequestedModel = requestedModelNames
            .FirstOrDefault(name => profiles.All(profile => !string.Equals(profile.ModelName, name, StringComparison.OrdinalIgnoreCase)));
        if (!string.IsNullOrWhiteSpace(missingRequestedModel))
        {
            throw new InvalidOperationException($"Model '{missingRequestedModel}' does not exist.");
        }

        var result = new Dictionary<string, ModelProfileData>(StringComparer.OrdinalIgnoreCase);

        foreach (var profile in profiles)
        {
            if (!requestedModelNames.Contains(profile.ModelName))
            {
                continue;
            }

            result[profile.ModelName] = BuildProfileData(
                profile.Id,
                profile.RobotData,
                localMachineSlotIndex == 2 ? profile.Line2Data : profile.Line1Data,
                profile.DiameterOp1);
        }

        return result;
    }

    private static ModelProfileData? ResolveProfileData(string? modelName, IReadOnlyDictionary<string, ModelProfileData> profileDataByModelName)
    {
        if (string.IsNullOrWhiteSpace(modelName))
        {
            return null;
        }

        return profileDataByModelName.TryGetValue(modelName.Trim(), out var data) ? data : null;
    }

    private static string? NormalizeOptionalReportModelName(string? reportModelName)
    {
        return string.IsNullOrWhiteSpace(reportModelName)
            ? null
            : reportModelName.Trim();
    }

    private static string ResolveReportModelName(string? reportModelName, string? articleId)
    {
        return NormalizeOptionalReportModelName(reportModelName)
            ?? articleId?.Trim()
            ?? string.Empty;
    }

    private static ModelProfileData BuildProfileData(
        int profileId,
        string? robotDataJson,
        string? lineDataJson,
        float? diameterOp1)
    {
        var robotData = ParseJsonObject(robotDataJson);
        var lineData = ParseJsonObject(lineDataJson);

        return new ModelProfileData(
            profileId,
            ReadIntOrDefault(lineData, "jigType"),
            ReadFloatOrDefault(robotData, "jigProductHeight"),
            ReadFloatOrDefault(robotData, "jigCenterOffset"),
            ReadFloatOrDefault(robotData, "jigDepthOffset"),
            diameterOp1);
    }

    private static Dictionary<string, JsonElement> ParseJsonObject(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
            }

            return document.RootElement
                .EnumerateObject()
                .ToDictionary(
                    property => property.Name,
                    property => property.Value.Clone(),
                    StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            return new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
        }
    }

    private static int ReadIntOrDefault(IReadOnlyDictionary<string, JsonElement> data, string key)
    {
        if (!data.TryGetValue(key, out var element))
        {
            return 0;
        }

        if (element.ValueKind == JsonValueKind.Number && element.TryGetInt32(out var value))
        {
            return value;
        }

        return element.ValueKind == JsonValueKind.String && int.TryParse(element.GetString(), out var parsed)
            ? parsed
            : 0;
    }

    private static float ReadFloatOrDefault(IReadOnlyDictionary<string, JsonElement> data, string key)
    {
        if (!data.TryGetValue(key, out var element))
        {
            return 0f;
        }

        if (element.ValueKind == JsonValueKind.Number && element.TryGetSingle(out var value))
        {
            return value;
        }

        return element.ValueKind == JsonValueKind.String && float.TryParse(element.GetString(), out var parsed)
            ? parsed
            : 0f;
    }

    private static ShelfDeclarationEventEntity CreateEvent(
        ManualShelfDeclarationEntity entity,
        string eventType,
        string actorType,
        string? actorId = null,
        string? actorName = null,
        DateTimeOffset? eventAtUtc = null,
        object? payload = null)
    {
        return new ShelfDeclarationEventEntity
        {
            Declaration = entity,
            EventType = eventType,
            EventAtUtc = eventAtUtc ?? DateTimeOffset.UtcNow,
            ActorType = actorType,
            ActorId = actorId,
            ActorName = actorName,
            MachineId = entity.MachineId,
            MachineSlotIndex = entity.MachineSlotIndex,
            StagingSlotIndex = entity.StagingSlotIndex,
            PayloadJson = payload is null ? null : JsonSerializer.Serialize(payload, JsonOptions)
        };
    }

    private static int CountOrders(string ordersJson)
    {
        try
        {
            var orders = JsonSerializer.Deserialize<JsonElement>(ordersJson);
            return orders.ValueKind == JsonValueKind.Array ? orders.GetArrayLength() : 0;
        }
        catch
        {
            return 0;
        }
    }

    private async Task<List<AgvSlotSnapshot>> GetActiveAgvSlotSnapshotsAsync(CancellationToken cancellationToken)
    {
        var active = await _db.ManualShelfDeclarations
            .Include(d => d.Machine)
            .Where(d => d.Mode == ShelfDeclarationModes.Agv
                        && d.Status == ShelfDeclarationStatuses.Created
                        && d.StagingSlotIndex.HasValue)
            .ToListAsync(cancellationToken);

        return Enumerable.Range(1, 4)
            .Select(slotIndex =>
            {
                var item = active.FirstOrDefault(x => x.StagingSlotIndex == slotIndex);
                return new AgvSlotSnapshot
                {
                    SlotIndex = slotIndex,
                    IsOccupied = item is not null,
                    DeclarationId = item?.Id,
                    MachineId = item?.MachineId,
                    MachineCode = item?.Machine?.MachineCode ?? item?.MachineCodeSnapshot,
                    MachineName = item?.Machine?.MachineName ?? item?.MachineNameSnapshot,
                    Status = item?.Status,
                    CreatedByUsername = item?.CreatedByUsername,
                    CreatedAtUtc = item?.CreatedAtUtc,
                    ShelfLayoutType = item?.ShelfLayoutType,
                    ShelfLayoutName = item is null ? null : GetLayoutName(item.ShelfLayoutType),
                    OrderCount = item is null ? null : CountOrders(item.OrdersJson)
                };
            })
            .ToList();
    }

    private static int? ComputeDurationSeconds(DateTimeOffset? start, DateTimeOffset? end)
    {
        if (!start.HasValue || !end.HasValue || end < start)
        {
            return null;
        }

        return (int)Math.Round((end.Value - start.Value).TotalSeconds, MidpointRounding.AwayFromZero);
    }

    private static ProductionLifecycleDeclarationResponse BuildLifecycleRow(ManualShelfDeclarationEntity entity, int? shelfIndex)
    {
        var parsedOrders = ParseOrders(entity.OrdersJson)
            .Select(o => new ProductionLifecycleOrderResponse
            {
                OrderId = string.IsNullOrWhiteSpace(o.OrderId) ? $"DECL-{entity.Id}-SEQ-{o.OrderSequence}" : o.OrderId!,
                OrderSequence = o.OrderSequence,
                ModelName = o.ModelName ?? string.Empty,
                ReportModelName = ResolveReportModelName(o.ReportModelName, o.ModelName),
                Quantity = o.Quantity,
                TrayIndex = o.TrayIndex,
                TrayType = o.TrayType,
                JigType = o.JigType,
                Status = string.IsNullOrWhiteSpace(o.Status) ? entity.Status : o.Status!
            })
            .OrderBy(x => x.OrderSequence)
            .ToArray();

        var lifecycleEnd = entity.CompletedAtUtc ?? entity.ClearedAtUtc ?? entity.CancelledAtUtc;
        var productionDuration = entity.ProductionDurationSeconds
            ?? ComputeDurationSeconds(entity.ProductionStartedAtUtc, entity.CompletedAtUtc ?? entity.ClearedAtUtc);

        return new ProductionLifecycleDeclarationResponse
        {
            DeclarationId = entity.Id,
            MachineId = entity.MachineId,
            MachineCode = entity.Machine?.MachineCode ?? entity.MachineCodeSnapshot,
            MachineName = entity.Machine?.MachineName ?? entity.MachineNameSnapshot,
            Mode = entity.Mode,
            ShelfIndex = shelfIndex,
            StagingSlotIndex = entity.StagingSlotIndex,
            Status = entity.Status,
            IsLoadingParameters = IsLoadingParameters(entity),
            CreatedAtUtc = entity.CreatedAtUtc,
            AgvTakenAtUtc = entity.AgvTakenAtUtc,
            LoadedAtUtc = entity.LoadedAtUtc,
            ProductionStartedAtUtc = entity.ProductionStartedAtUtc,
            CompletedAtUtc = entity.CompletedAtUtc,
            ClearedAtUtc = entity.ClearedAtUtc,
            CancelledAtUtc = entity.CancelledAtUtc,
            AgvPickupDurationSeconds = ComputeDurationSeconds(entity.CreatedAtUtc, entity.AgvTakenAtUtc),
            ProductionDurationSeconds = productionDuration,
            LifecycleDurationSeconds = ComputeDurationSeconds(entity.CreatedAtUtc, lifecycleEnd),
            Orders = parsedOrders
        };
    }

    private static bool IsInProgressStatus(string status)
    {
        return string.Equals(status, ShelfDeclarationStatuses.Created, StringComparison.OrdinalIgnoreCase)
               || string.Equals(status, ShelfDeclarationStatuses.AgvTaken, StringComparison.OrdinalIgnoreCase)
               || string.Equals(status, ShelfDeclarationStatuses.Loaded, StringComparison.OrdinalIgnoreCase)
               || string.Equals(status, ShelfDeclarationStatuses.InProduction, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsLoadingParameters(ManualShelfDeclarationEntity entity)
    {
        if (string.Equals(entity.Status, ShelfDeclarationStatuses.AgvTaken, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.Equals(entity.Status, ShelfDeclarationStatuses.Loaded, StringComparison.OrdinalIgnoreCase)
            && !entity.ProductionStartedAtUtc.HasValue)
        {
            return true;
        }

        return false;
    }

    private static int? ResolveShelfIndex(ManualShelfDeclarationEntity entity)
    {
        if (entity.Mode == ShelfDeclarationModes.ManualLoad)
        {
            return entity.MachineSlotIndex;
        }

        if (entity.MachineSlotIndex.HasValue)
        {
            return entity.MachineSlotIndex;
        }

        if (entity.StagingSlotIndex.HasValue && entity.Machine is not null)
        {
            return GetLocalMachineSlotIndex(entity.Machine, entity.StagingSlotIndex);
        }

        return null;
    }

    private static IReadOnlyList<ShelfOrderSnapshot> ParseOrders(string ordersJson)
    {
        try
        {
            var orders = JsonSerializer.Deserialize<List<ShelfOrderSnapshot>>(ordersJson, JsonOptions);
            return orders ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static HashSet<int> GetAssignedStagingSlots(MachineEntity machine)
    {
        return new[] { machine.AssignedStagingSlot1, machine.AssignedStagingSlot2 }
            .Where(slot => slot.HasValue)
            .Select(slot => slot!.Value)
            .ToHashSet();
    }

    private static int[] GetOrderedAssignedStagingSlots(MachineEntity machine)
    {
        var slots = GetAssignedStagingSlots(machine)
            .OrderBy(slot => slot)
            .ToArray();
        if (slots.Length != 2)
        {
            throw new InvalidOperationException($"Machine {machine.MachineCode} must be assigned exactly 2 staging slots.");
        }

        return slots;
    }

    private static int GetStagingSlotForLocalMachineSlot(MachineEntity machine, int machineSlotIndex)
    {
        if (machineSlotIndex is < 1 or > 2)
        {
            throw new InvalidOperationException("machineSlotIndex must be between 1 and 2.");
        }

        var slots = GetOrderedAssignedStagingSlots(machine);
        return slots[machineSlotIndex - 1];
    }

    private static int ValidateAgvPickupSlotIndexes(MachineEntity machine, int machineSlotIndex, int? slotIndex)
    {
        if (machineSlotIndex is < 1 or > 2)
        {
            throw new InvalidOperationException("machineSlotIndex must be between 1 and 2.");
        }

        var expectedSlotIndex = GetStagingSlotForLocalMachineSlot(machine, machineSlotIndex);
        if (!slotIndex.HasValue)
        {
            return expectedSlotIndex;
        }

        if (slotIndex.Value is < 1 or > 4)
        {
            throw new InvalidOperationException("slotIndex must be between 1 and 4.");
        }

        var assignedSlots = GetAssignedStagingSlots(machine);
        if (!assignedSlots.Contains(slotIndex.Value))
        {
            throw new InvalidOperationException($"Staging slot {slotIndex.Value} does not belong to machine {machine.MachineCode}.");
        }

        if (slotIndex.Value != expectedSlotIndex)
        {
            throw new InvalidOperationException(
                $"slotIndex {slotIndex.Value} does not match machineSlotIndex {machineSlotIndex} for machine {machine.MachineCode}.");
        }

        return expectedSlotIndex;
    }

    private static int? GetLocalMachineSlotIndex(MachineEntity machine, int? stagingSlotIndex)
    {
        if (!stagingSlotIndex.HasValue)
        {
            return null;
        }

        var slots = GetOrderedAssignedStagingSlots(machine);
        return Array.IndexOf(slots, stagingSlotIndex.Value) switch
        {
            0 => 1,
            1 => 2,
            _ => throw new InvalidOperationException(
                $"Staging slot {stagingSlotIndex.Value} does not belong to machine {machine.MachineCode}.")
        };
    }

    public static string GetLayoutName(int shelfLayoutType) => shelfLayoutType switch
    {
        1 => "2 Tray Nho",
        2 => "2 Tray Lon",
        3 => "Nho duoi + Lon tren",
        4 => "Lon duoi + Nho tren",
        _ => "Khong xac dinh"
    };

    private sealed class AgvSlotSnapshot
    {
        public int SlotIndex { get; init; }
        public bool IsOccupied { get; init; }
        public int? DeclarationId { get; init; }
        public int? MachineId { get; init; }
        public string? MachineCode { get; init; }
        public string? MachineName { get; init; }
        public string? Status { get; init; }
        public string? CreatedByUsername { get; init; }
        public DateTimeOffset? CreatedAtUtc { get; init; }
        public int? ShelfLayoutType { get; init; }
        public string? ShelfLayoutName { get; init; }
        public int? OrderCount { get; init; }
    }

    private sealed class ShelfOrderSnapshot
    {
        public string? OrderId { get; set; }
        public string? ModelName { get; set; }
        public string? ReportModelName { get; set; }
        public int Quantity { get; set; }
        public int TrayIndex { get; set; }
        public int TrayType { get; set; }
        public int OrderSequence { get; set; }
        public int JigType { get; set; }
        public float? PartHoverHeight { get; set; }
        public float? JigCenterOffset { get; set; }
        public float? JigDepthOffset { get; set; }
        public float? DiameterOp1 { get; set; }
        public string? Status { get; set; }
        public DateTimeOffset? CompletedAtUtc { get; set; }
    }
}
