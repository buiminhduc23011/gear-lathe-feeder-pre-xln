using Microsoft.EntityFrameworkCore;
using Server.Api.Contracts.Requests;
using Server.Api.Data;
using Server.Api.Data.Entities;
using Server.Api.Exceptions;

namespace Server.Api.Services;

public sealed class MachineService : IMachineService
{
    private readonly AppDbContext _dbContext;

    public MachineService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<MachineEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Machines
            .OrderBy(x => x.MachineCode)
            .AsNoTracking()
            .ToArrayAsync(cancellationToken);
    }

    public async Task<MachineEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Machines
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.MachineId == id, cancellationToken);
    }

    public async Task<MachineEntity> CreateAsync(CreateMachineRequest request, CancellationToken cancellationToken = default)
    {
        var stagingSlots = await ValidateRequestAsync(request.MachineCode, request.StagingSlotIndices, null, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var entity = new MachineEntity
        {
            MachineCode = request.MachineCode.Trim(),
            MachineName = request.MachineName.Trim(),
            Manufacturer = request.Manufacturer.Trim(),
            Description = request.Description.Trim(),
            Model = NormalizeOptional(request.Model),
            SerialNumber = NormalizeOptional(request.SerialNumber),
            Location = NormalizeOptional(request.Location),
            Jig1HeightMm = request.Jig1HeightMm,
            Jig2HeightMm = request.Jig2HeightMm,
            Jig3HeightMm = request.Jig3HeightMm,
            Jig4HeightMm = request.Jig4HeightMm,
            AssignedStagingSlot1 = stagingSlots[0],
            AssignedStagingSlot2 = null,
            IsActive = request.IsActive,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        _dbContext.Machines.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<MachineEntity?> UpdateAsync(int id, UpdateMachineRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Machines.SingleOrDefaultAsync(x => x.MachineId == id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        var stagingSlots = await ValidateRequestAsync(request.MachineCode, request.StagingSlotIndices, id, cancellationToken);

        entity.MachineCode = request.MachineCode.Trim();
        entity.MachineName = request.MachineName.Trim();
        entity.Manufacturer = request.Manufacturer.Trim();
        entity.Description = request.Description.Trim();
        entity.Model = NormalizeOptional(request.Model);
        entity.SerialNumber = NormalizeOptional(request.SerialNumber);
        entity.Location = NormalizeOptional(request.Location);
        entity.Jig1HeightMm = request.Jig1HeightMm;
        entity.Jig2HeightMm = request.Jig2HeightMm;
        entity.Jig3HeightMm = request.Jig3HeightMm;
        entity.Jig4HeightMm = request.Jig4HeightMm;
        entity.AssignedStagingSlot1 = stagingSlots[0];
        entity.AssignedStagingSlot2 = null;
        entity.IsActive = request.IsActive;
        entity.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Machines.SingleOrDefaultAsync(x => x.MachineId == id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        _dbContext.Machines.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<int[]> ValidateRequestAsync(
        string machineCodeInput,
        IEnumerable<int>? stagingSlotIndices,
        int? existingId,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        var machineCode = machineCodeInput.Trim();
        var slots = NormalizeStagingSlots(stagingSlotIndices, errors);

        if (await _dbContext.Machines.AnyAsync(
                x => x.MachineCode == machineCode && (!existingId.HasValue || x.MachineId != existingId.Value),
                cancellationToken))
        {
            errors["machineCode"] = ["MachineCode must be unique."];
        }

        if (slots.Length == 1)
        {
            var assignedSlots = await _dbContext.Machines
                .Where(x => !existingId.HasValue || x.MachineId != existingId.Value)
                .Select(x => x.AssignedStagingSlot1)
                .ToListAsync(cancellationToken);

            var overlaps = assignedSlots
                .Where(slot => slot.HasValue && slots.Contains(slot.Value))
                .Select(slot => slot!.Value)
                .Distinct()
                .OrderBy(slot => slot)
                .ToArray();

            if (overlaps.Length > 0)
            {
                errors["stagingSlotIndices"] = [$"Staging slot {string.Join(", ", overlaps)} is already assigned to another machine."];
            }
        }

        if (errors.Count > 0)
        {
            throw new ValidationProblemException(errors);
        }

        return slots;
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static int[] NormalizeStagingSlots(IEnumerable<int>? stagingSlotIndices, IDictionary<string, string[]> errors)
    {
        var slots = stagingSlotIndices?.ToArray() ?? Array.Empty<int>();

        if (slots.Length != 1)
        {
            errors["stagingSlotIndices"] = ["Each machine must be assigned exactly 1 staging slot."];
            return Array.Empty<int>();
        }

        if (slots.Any(slot => slot < 1 || slot > 4))
        {
            errors["stagingSlotIndices"] = ["Staging slots must be between 1 and 4."];
            return Array.Empty<int>();
        }

        var distinctSlots = slots.Distinct().OrderBy(slot => slot).ToArray();
        if (distinctSlots.Length != 1)
        {
            errors["stagingSlotIndices"] = ["Staging slots must be unique for each machine."];
            return Array.Empty<int>();
        }

        return distinctSlots;
    }
}
