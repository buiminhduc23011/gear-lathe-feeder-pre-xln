using System.Globalization;
using System.Text.Json;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Server.Api.Data;
using Server.Api.Data.Entities;
using Server.Api.Exceptions;
using Shared.Models.ModelProfiles;

namespace Server.Api.Services;

public sealed class ModelProfileService : IModelProfileService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private const string ModelsSheetName = "Models";
    private const int ModelsHeaderRow = 2;
    private const int ModelsDataStartRow = 3;
    private const int ModelsMetadataColumnCount = 8;
    private const int ModelsRobotStartColumn = ModelsMetadataColumnCount + 1;
    private const string LegacyTrayUsageHeader = "Tray sử dụng";
    private const string ExpectedOrderInputHeader = "Nhập order";
    private const int ExcelHeaderWrapThreshold = 18;
    private const double ExcelMinParameterColumnWidth = 8.5;
    private const double ExcelMaxWrappedColumnWidth = 12;
    private const double ExcelGroupHeaderRowHeight = 22;
    private const double ExcelDetailHeaderRowHeight = 48;

    private static readonly ExcelMetadataField[] MetadataFieldDefinitions =
    [
        new(nameof(ModelProfileEntity.ItemType), "Loại hàng", 2, ExcelMetadataKind.Text),
        new(nameof(ModelProfileEntity.MachiningProgram), "Chương trình gia công", 3, ExcelMetadataKind.Int),
        new(nameof(ModelProfileEntity.OuterShaftDiameter), "Đường kính ngoài trục", 4, ExcelMetadataKind.Decimal),
        new(nameof(ModelProfileEntity.DiameterOp1), "Đường kính Op1", 5, ExcelMetadataKind.Float),
        new(nameof(ModelProfileEntity.DiameterOp2), "Đường kính Op2", 6, ExcelMetadataKind.Float),
        new(nameof(ModelProfileEntity.TrayType), "Loại tray", 7, ExcelMetadataKind.TrayType),
        new(nameof(ModelProfileEntity.OrderInput), ExpectedOrderInputHeader, 8, ExcelMetadataKind.OrderInput)
    ];

    private readonly AppDbContext _dbContext;

    public ModelProfileService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ModelProfileEntity>> GetByMachineAsync(int machineId, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.ModelProfiles
            .Where(x => x.MachineId == machineId);

        if (!includeDeleted)
        {
            query = query.Where(x => !x.IsDeleted);
        }

        return await query
            .OrderBy(x => x.ModelName)
            .AsNoTracking()
            .ToArrayAsync(cancellationToken);
    }

    public async Task<ModelProfileEntity?> GetByIdAsync(int machineId, int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ModelProfiles
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id && x.MachineId == machineId, cancellationToken);
    }

    public async Task<ModelProfileEntity?> GetByNameAsync(int machineId, string modelName, CancellationToken cancellationToken = default)
    {
        var name = modelName.Trim();
        return await _dbContext.ModelProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.MachineId == machineId && !x.IsDeleted && x.ModelName.ToLower() == name.ToLower(),
                cancellationToken);
    }

    public async Task<ModelProfileEntity> CreateAsync(int machineId, SaveModelProfileRequest request, string username, CancellationToken cancellationToken = default)
    {
        var modelName = (request.ModelName ?? string.Empty).Trim();
        ValidateModelName(modelName);
        ValidateRequestMetadata(request);
        await ValidateUniqueNameAsync(machineId, modelName, null, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var requestRobotData = request.RobotData ?? [];
        var isActivationReady = HasActivationPrerequisites(DefaultFloat(request.DiameterOp1), DefaultFloat(request.DiameterOp2), requestRobotData);

        // Check if a soft-deleted model with the same name exists – reactivate it to keep history grouped.
        var deleted = await _dbContext.ModelProfiles
            .SingleOrDefaultAsync(x => x.MachineId == machineId && x.ModelName == modelName && x.IsDeleted, cancellationToken);

        if (deleted is not null)
        {
            ApplyRequestMetadata(deleted, request);
            deleted.RobotData = SerializeData(requestRobotData);
            deleted.Line1Data = SerializeData(request.Line1Data);
            deleted.Line2Data = SerializeData(request.Line2Data);
            deleted.IsEnabled = deleted.IsEnabled && isActivationReady;
            deleted.IsDeleted = false;
            deleted.DeletedAtUtc = null;
            deleted.DeletedByUsername = null;
            deleted.UpdatedByUsername = username;
            deleted.UpdatedAtUtc = now;

            _dbContext.ModelProfileSnapshots.Add(CreateSnapshot(deleted, "Created", username));
            await _dbContext.SaveChangesAsync(cancellationToken);
            return deleted;
        }

        var entity = new ModelProfileEntity
        {
            MachineId = machineId,
            ModelName = modelName,
            ItemType = TrimToNull(request.ItemType),
            MachiningProgram = DefaultInt(request.MachiningProgram),
            Spare1 = TrimToNull(request.Spare1),
            Spare2 = TrimToNull(request.Spare2),
            OuterShaftDiameter = DefaultDecimal(request.OuterShaftDiameter),
            DiameterOp1 = DefaultFloat(request.DiameterOp1),
            DiameterOp2 = DefaultFloat(request.DiameterOp2),
            TrayUsage = DefaultInt(request.TrayUsage),
            TrayType = DefaultTrayType(request.TrayType),
            OrderInput = DefaultOrderInput(request.OrderInput),
            RobotData = SerializeData(requestRobotData),
            Line1Data = SerializeData(request.Line1Data),
            Line2Data = SerializeData(request.Line2Data),
            CreatedByUsername = username,
            UpdatedByUsername = username,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        _dbContext.ModelProfiles.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _dbContext.ModelProfileSnapshots.Add(CreateSnapshot(entity, "Created", username));
        await _dbContext.SaveChangesAsync(cancellationToken);

        return entity;
    }

    public async Task<ModelProfileEntity?> UpdateAsync(int machineId, int id, SaveModelProfileRequest request, string username, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.ModelProfiles.SingleOrDefaultAsync(x => x.Id == id && x.MachineId == machineId, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        var modelName = (request.ModelName ?? string.Empty).Trim();
        ValidateModelName(modelName);
        ValidateRequestMetadata(request);
        await ValidateUniqueNameAsync(entity.MachineId, modelName, id, cancellationToken);

        var nextRobotData = SerializeData(request.RobotData);
        var nextLine1Data = SerializeData(request.Line1Data);
        var nextLine2Data = SerializeData(request.Line2Data);
        var hasChanges = entity.ModelName != modelName
            || !string.Equals(entity.RobotData, nextRobotData, StringComparison.Ordinal)
            || !string.Equals(entity.Line1Data, nextLine1Data, StringComparison.Ordinal)
            || !string.Equals(entity.Line2Data, nextLine2Data, StringComparison.Ordinal)
            || !RequestMetadataEquals(entity, request);

        if (!hasChanges)
        {
            return entity;
        }

        if (entity.IsEnabled)
        {
            EnsureActivationPrerequisites(DefaultFloat(request.DiameterOp1), DefaultFloat(request.DiameterOp2), request.RobotData ?? []);
        }

        // Snapshot the current state before applying changes.
        _dbContext.ModelProfileSnapshots.Add(CreateSnapshot(entity, "Updated", username));

        entity.ModelName = modelName;
        ApplyRequestMetadata(entity, request);
        entity.RobotData = nextRobotData;
        entity.Line1Data = nextLine1Data;
        entity.Line2Data = nextLine2Data;
        entity.UpdatedByUsername = username;
        entity.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<bool> DeleteAsync(int machineId, int id, string username, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.ModelProfiles.SingleOrDefaultAsync(x => x.Id == id && x.MachineId == machineId, cancellationToken);
        if (entity is null || entity.IsDeleted)
        {
            return false;
        }

        _dbContext.ModelProfileSnapshots.Add(CreateSnapshot(entity, "Deleted", username));

        entity.IsDeleted = true;
        entity.DeletedAtUtc = DateTimeOffset.UtcNow;
        entity.DeletedByUsername = username;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<ModelProfileEntity?> SetEnabledAsync(int machineId, int id, bool isEnabled, string username, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.ModelProfiles.SingleOrDefaultAsync(x => x.Id == id && x.MachineId == machineId && !x.IsDeleted, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        if (isEnabled)
        {
            EnsureActivationPrerequisites(entity);
        }

        var action = isEnabled ? "Enabled" : "Disabled";
        _dbContext.ModelProfileSnapshots.Add(CreateSnapshot(entity, action, username));

        entity.IsEnabled = isEnabled;
        entity.UpdatedByUsername = username;
        entity.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<IReadOnlyList<ModelProfileSnapshotEntity>> GetSnapshotsAsync(int modelProfileId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ModelProfileSnapshots
            .Where(x => x.ModelProfileId == modelProfileId)
            .OrderByDescending(x => x.PerformedAtUtc)
            .AsNoTracking()
            .ToArrayAsync(cancellationToken);
    }

    public async Task<byte[]> ExportExcelAsync(int machineId, CancellationToken cancellationToken = default)
    {
        var profiles = await GetByMachineAsync(machineId, false, cancellationToken);

        using var workbook = new XLWorkbook();
        WriteModelsSheet(workbook, profiles);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public Task<int> UpdateExcelAsync(int machineId, Stream stream, string username, CancellationToken cancellationToken = default)
    {
        return ImportExcelCoreAsync(machineId, stream, username, replaceAll: false, cancellationToken);
    }

    public Task<int> ReplaceExcelAsync(int machineId, Stream stream, string username, CancellationToken cancellationToken = default)
    {
        return ImportExcelCoreAsync(machineId, stream, username, replaceAll: true, cancellationToken);
    }

    private async Task<int> ImportExcelCoreAsync(
        int machineId,
        Stream stream,
        string username,
        bool replaceAll,
        CancellationToken cancellationToken = default)
    {
        using var workbook = new XLWorkbook(stream);
        var validationErrors = new List<ModelExcelValidationError>();
        var modelRows = ReadModelsSheet(workbook, validationErrors);

        if (validationErrors.Count > 0)
        {
            throw new ModelExcelValidationException(validationErrors);
        }

        var allNames = modelRows.Rows.Keys
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (allNames.Length == 0)
        {
            throw new ModelExcelValidationException(
            [
                new ModelExcelValidationError
                {
                    Sheet = ModelsSheetName,
                    Field = "Article ID",
                    Message = "Sheet Models không có dòng model nào."
                }
            ]);
        }

        var existingProfiles = await _dbContext.ModelProfiles
            .Where(x => x.MachineId == machineId)
            .ToDictionaryAsync(x => x.ModelName, x => x, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var count = 0;
        var newEntities = new List<ModelProfileEntity>();
        var incomingNames = new HashSet<string>(allNames, StringComparer.OrdinalIgnoreCase);

        foreach (var name in allNames)
        {
            var row = modelRows.Rows[name];
            var robotData = row.RobotData;
            var line1Data = row.Line1Data;
            var line2Data = row.Line2Data;
            var incomingMetadata = BuildIncomingMetadata(row);

            if (existingProfiles.TryGetValue(name, out var existing))
            {
                var nextRobotData = replaceAll ? robotData : MergeData(existing.RobotData, robotData);
                var nextLine1Data = replaceAll ? line1Data : MergeData(existing.Line1Data, line1Data);
                var nextLine2Data = replaceAll ? line2Data : MergeData(existing.Line2Data, line2Data);
                var nextMetadata = BuildNextMetadata(existing, incomingMetadata, replaceAll);
                var hasDataChanges =
                    !DataEquals(existing.RobotData, nextRobotData)
                    || !DataEquals(existing.Line1Data, nextLine1Data)
                    || !DataEquals(existing.Line2Data, nextLine2Data)
                    || !MetadataEquals(existing, nextMetadata);

                if (!existing.IsDeleted && !hasDataChanges)
                {
                    continue;
                }

                if (existing.IsEnabled)
                {
                    var missingActivationFields = GetMissingActivationFields(nextMetadata.DiameterOp1, nextMetadata.DiameterOp2, nextRobotData);
                    if (missingActivationFields.Count > 0)
                    {
                        validationErrors.Add(CreateActivationExcelValidationError(row, missingActivationFields));
                        continue;
                    }
                }

                var action = existing.IsDeleted ? "Created" : "Updated";
                _dbContext.ModelProfileSnapshots.Add(CreateSnapshot(existing, action, username));
                if (existing.IsDeleted)
                {
                    existing.IsDeleted = false;
                    existing.DeletedAtUtc = null;
                    existing.DeletedByUsername = null;
                }
                existing.RobotData = SerializeData(nextRobotData);
                existing.Line1Data = SerializeData(nextLine1Data);
                existing.Line2Data = SerializeData(nextLine2Data);
                ApplyMetadata(existing, nextMetadata);
                existing.UpdatedByUsername = username;
                existing.UpdatedAtUtc = now;
                count++;
            }
            else
            {
                var newMetadata = WithDefaultMetadata(incomingMetadata);
                var entity = new ModelProfileEntity
                {
                    MachineId = machineId,
                    ModelName = name.Trim(),
                    ItemType = newMetadata.ItemType,
                    MachiningProgram = newMetadata.MachiningProgram,
                    Spare1 = newMetadata.Spare1,
                    Spare2 = newMetadata.Spare2,
                    OuterShaftDiameter = newMetadata.OuterShaftDiameter,
                    DiameterOp1 = newMetadata.DiameterOp1,
                    DiameterOp2 = newMetadata.DiameterOp2,
                    TrayUsage = newMetadata.TrayUsage,
                    TrayType = newMetadata.TrayType,
                    OrderInput = newMetadata.OrderInput,
                    RobotData = SerializeData(robotData),
                    Line1Data = SerializeData(line1Data),
                    Line2Data = SerializeData(line2Data),
                    CreatedByUsername = username,
                    UpdatedByUsername = username,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now,
                    IsEnabled = HasActivationPrerequisites(newMetadata.DiameterOp1, newMetadata.DiameterOp2, robotData)
                };

                _dbContext.ModelProfiles.Add(entity);
                newEntities.Add(entity);
                count++;
            }
        }

        if (validationErrors.Count > 0)
        {
            throw new ModelExcelValidationException(validationErrors);
        }

        if (replaceAll)
        {
            foreach (var existing in existingProfiles.Values)
            {
                if (existing.IsDeleted || incomingNames.Contains(existing.ModelName))
                {
                    continue;
                }

                _dbContext.ModelProfileSnapshots.Add(CreateSnapshot(existing, "Deleted", username));

                existing.IsDeleted = true;
                existing.DeletedAtUtc = now;
                existing.DeletedByUsername = username;
                existing.UpdatedByUsername = username;
                existing.UpdatedAtUtc = now;
                count++;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        // Create initial snapshot for newly imported models (ensures complete audit history like manual CreateAsync).
        foreach (var entity in newEntities)
        {
            _dbContext.ModelProfileSnapshots.Add(CreateSnapshot(entity, "Created", username));
        }
        if (newEntities.Count > 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return count;
    }

    // ───── Helpers ─────

    private static void ValidateModelName(string modelName)
    {
        if (string.IsNullOrWhiteSpace(modelName))
        {
            throw new ValidationProblemException(new Dictionary<string, string[]>
            {
                ["modelName"] = ["Article ID không được để trống."]
            });
        }
    }

    private static void ValidateRequestMetadata(SaveModelProfileRequest request)
    {
        if (request.TrayType is not null and not (0 or 1))
        {
            throw new ValidationProblemException(new Dictionary<string, string[]>
            {
                ["trayType"] = ["Loại tray chỉ được nhập 0 (Nhỏ) hoặc 1 (To)."]
            });
        }

        if (request.OrderInput is not null and not (0 or 1))
        {
            throw new ValidationProblemException(new Dictionary<string, string[]>
            {
                ["orderInput"] = ["Nhập order chỉ được nhập 0 (Không nhập) hoặc 1 (Nhập)."]
            });
        }
    }

    private static void EnsureActivationPrerequisites(ModelProfileEntity entity)
    {
        EnsureActivationPrerequisites(entity.DiameterOp1, entity.DiameterOp2, DeserializeData(entity.RobotData));
    }

    private static void EnsureActivationPrerequisites(float? diameterOp1, float? diameterOp2, Dictionary<string, object?> robotData)
    {
        var missingFields = GetMissingActivationFields(diameterOp1, diameterOp2, robotData);
        if (missingFields.Count == 0)
        {
            return;
        }

        throw new ValidationProblemException(new Dictionary<string, string[]>
        {
            ["isEnabled"] =
            [
                $"Chỉ có thể Active khi đã nhập đủ và > 0 cho các thông số bắt buộc: {string.Join(", ", missingFields)}."
            ]
        });
    }

    private static void ApplyRequestMetadata(ModelProfileEntity entity, SaveModelProfileRequest request)
    {
        entity.ItemType = TrimToNull(request.ItemType);
        entity.MachiningProgram = DefaultInt(request.MachiningProgram);
        entity.Spare1 = TrimToNull(request.Spare1);
        entity.Spare2 = TrimToNull(request.Spare2);
        entity.OuterShaftDiameter = DefaultDecimal(request.OuterShaftDiameter);
        entity.DiameterOp1 = DefaultFloat(request.DiameterOp1);
        entity.DiameterOp2 = DefaultFloat(request.DiameterOp2);
        entity.TrayUsage = request.TrayUsage ?? entity.TrayUsage ?? 0;
        entity.TrayType = DefaultTrayType(request.TrayType);
        entity.OrderInput = request.OrderInput ?? entity.OrderInput ?? 1;
    }

    private static bool RequestMetadataEquals(ModelProfileEntity entity, SaveModelProfileRequest request)
    {
        return string.Equals(entity.ItemType, TrimToNull(request.ItemType), StringComparison.Ordinal)
            && entity.MachiningProgram == DefaultInt(request.MachiningProgram)
            && string.Equals(entity.Spare1, TrimToNull(request.Spare1), StringComparison.Ordinal)
            && string.Equals(entity.Spare2, TrimToNull(request.Spare2), StringComparison.Ordinal)
            && entity.OuterShaftDiameter == DefaultDecimal(request.OuterShaftDiameter)
            && entity.DiameterOp1 == DefaultFloat(request.DiameterOp1)
            && entity.DiameterOp2 == DefaultFloat(request.DiameterOp2)
            && entity.TrayUsage == (request.TrayUsage ?? entity.TrayUsage ?? 0)
            && entity.TrayType == DefaultTrayType(request.TrayType)
            && entity.OrderInput == (request.OrderInput ?? entity.OrderInput ?? 1);
    }

    private static string? TrimToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static int DefaultInt(int? value)
    {
        return value ?? 0;
    }

    private static decimal DefaultDecimal(decimal? value)
    {
        return value ?? 0m;
    }

    private static float DefaultFloat(float? value)
    {
        return value ?? 0f;
    }

    private static int DefaultTrayType(int? value)
    {
        return value ?? 0;
    }

    private static int DefaultOrderInput(int? value)
    {
        return value ?? 1;
    }

    private static IncomingModelMetadata WithDefaultMetadata(IncomingModelMetadata metadata)
    {
        metadata.MachiningProgram ??= 0;
        metadata.HasMachiningProgram = true;
        metadata.OuterShaftDiameter ??= 0m;
        metadata.HasOuterShaftDiameter = true;
        metadata.DiameterOp1 ??= 0f;
        metadata.HasDiameterOp1 = true;
        metadata.DiameterOp2 ??= 0f;
        metadata.HasDiameterOp2 = true;
        metadata.TrayUsage ??= 0;
        metadata.HasTrayUsage = true;
        metadata.TrayType ??= 0;
        metadata.HasTrayType = true;
        metadata.OrderInput ??= 1;
        metadata.HasOrderInput = true;
        return metadata;
    }

    private static IncomingModelMetadata BuildIncomingMetadata(ExcelModelRow row)
    {
        var metadata = new IncomingModelMetadata();

        foreach (var item in row.Metadata.Values)
        {
            ApplyIncomingMetadataValue(metadata, item);
        }

        return metadata;
    }

    private static bool HasActivationPrerequisites(float? diameterOp1, float? diameterOp2, Dictionary<string, object?> robotData)
    {
        return GetMissingActivationFields(diameterOp1, diameterOp2, robotData).Count == 0;
    }

    private static List<string> GetMissingActivationFields(float? diameterOp1, float? diameterOp2, IReadOnlyDictionary<string, object?> robotData)
    {
        var missingFields = new List<string>();

        if (!HasPositiveValue(diameterOp1))
        {
            missingFields.Add("Đường kính Op1");
        }

        if (!HasPositiveValue(diameterOp2))
        {
            missingFields.Add("Đường kính Op2");
        }

        robotData.TryGetValue("jigProductHeight", out var jigProductHeight);
        if (!HasPositiveValue(jigProductHeight))
        {
            missingFields.Add("Độ cao trên Jig");
        }

        return missingFields;
    }

    private static IncomingModelMetadata BuildNextMetadata(ModelProfileEntity existing, IncomingModelMetadata incoming, bool replaceAll)
    {
        if (replaceAll)
        {
            var replacement = WithDefaultMetadata(incoming);
            replacement.TrayUsage = existing.TrayUsage;
            replacement.HasTrayUsage = true;
            return replacement;
        }

        return new IncomingModelMetadata
        {
            ItemType = incoming.HasItemType ? incoming.ItemType : existing.ItemType,
            HasItemType = true,
            MachiningProgram = incoming.HasMachiningProgram ? incoming.MachiningProgram : existing.MachiningProgram,
            HasMachiningProgram = true,
            Spare1 = incoming.HasSpare1 ? incoming.Spare1 : existing.Spare1,
            HasSpare1 = true,
            Spare2 = incoming.HasSpare2 ? incoming.Spare2 : existing.Spare2,
            HasSpare2 = true,
            OuterShaftDiameter = incoming.HasOuterShaftDiameter ? incoming.OuterShaftDiameter : existing.OuterShaftDiameter,
            HasOuterShaftDiameter = true,
            DiameterOp1 = incoming.HasDiameterOp1 ? incoming.DiameterOp1 : existing.DiameterOp1,
            HasDiameterOp1 = true,
            DiameterOp2 = incoming.HasDiameterOp2 ? incoming.DiameterOp2 : existing.DiameterOp2,
            HasDiameterOp2 = true,
            TrayUsage = existing.TrayUsage,
            HasTrayUsage = true,
            TrayType = incoming.HasTrayType ? incoming.TrayType : existing.TrayType,
            HasTrayType = true,
            OrderInput = incoming.HasOrderInput ? incoming.OrderInput : existing.OrderInput,
            HasOrderInput = true
        };
    }

    private static void ApplyMetadata(ModelProfileEntity entity, IncomingModelMetadata metadata)
    {
        entity.ItemType = metadata.ItemType;
        entity.MachiningProgram = metadata.MachiningProgram;
        entity.Spare1 = metadata.Spare1;
        entity.Spare2 = metadata.Spare2;
        entity.OuterShaftDiameter = metadata.OuterShaftDiameter;
        entity.DiameterOp1 = metadata.DiameterOp1;
        entity.DiameterOp2 = metadata.DiameterOp2;
        entity.TrayUsage = metadata.TrayUsage;
        entity.TrayType = metadata.TrayType;
        entity.OrderInput = metadata.OrderInput;
    }

    private static bool MetadataEquals(ModelProfileEntity entity, IncomingModelMetadata metadata)
    {
        return string.Equals(entity.ItemType, metadata.ItemType, StringComparison.Ordinal)
            && entity.MachiningProgram == metadata.MachiningProgram
            && string.Equals(entity.Spare1, metadata.Spare1, StringComparison.Ordinal)
            && string.Equals(entity.Spare2, metadata.Spare2, StringComparison.Ordinal)
            && entity.OuterShaftDiameter == metadata.OuterShaftDiameter
            && entity.DiameterOp1 == metadata.DiameterOp1
            && entity.DiameterOp2 == metadata.DiameterOp2
            && entity.TrayUsage == metadata.TrayUsage
            && entity.TrayType == metadata.TrayType
            && entity.OrderInput == metadata.OrderInput;
    }

    private static void ApplyIncomingMetadataValue(IncomingModelMetadata metadata, ExcelMetadataValue item)
    {
        switch (item.Field.Key)
        {
            case nameof(ModelProfileEntity.ItemType):
                metadata.ItemType = (string?)item.Value;
                metadata.HasItemType = true;
                break;
            case nameof(ModelProfileEntity.MachiningProgram):
                metadata.MachiningProgram = (int?)item.Value;
                metadata.HasMachiningProgram = true;
                break;
            case nameof(ModelProfileEntity.Spare1):
                metadata.Spare1 = (string?)item.Value;
                metadata.HasSpare1 = true;
                break;
            case nameof(ModelProfileEntity.Spare2):
                metadata.Spare2 = (string?)item.Value;
                metadata.HasSpare2 = true;
                break;
            case nameof(ModelProfileEntity.OuterShaftDiameter):
                metadata.OuterShaftDiameter = (decimal?)item.Value;
                metadata.HasOuterShaftDiameter = true;
                break;
            case nameof(ModelProfileEntity.DiameterOp1):
                metadata.DiameterOp1 = (float?)item.Value;
                metadata.HasDiameterOp1 = true;
                break;
            case nameof(ModelProfileEntity.DiameterOp2):
                metadata.DiameterOp2 = (float?)item.Value;
                metadata.HasDiameterOp2 = true;
                break;
            case nameof(ModelProfileEntity.TrayType):
                metadata.TrayType = (int?)item.Value;
                metadata.HasTrayType = true;
                break;
            case nameof(ModelProfileEntity.OrderInput):
                metadata.OrderInput = (int?)item.Value;
                metadata.HasOrderInput = true;
                break;
        }
    }

    private static ModelProfileSnapshotEntity CreateSnapshot(ModelProfileEntity entity, string action, string username)
    {
        return new ModelProfileSnapshotEntity
        {
            ModelProfileId = entity.Id,
            ModelName = entity.ModelName,
            ItemType = entity.ItemType,
            MachiningProgram = entity.MachiningProgram,
            Spare1 = entity.Spare1,
            Spare2 = entity.Spare2,
            OuterShaftDiameter = entity.OuterShaftDiameter,
            DiameterOp1 = entity.DiameterOp1,
            DiameterOp2 = entity.DiameterOp2,
            TrayUsage = entity.TrayUsage,
            TrayType = entity.TrayType,
            OrderInput = entity.OrderInput,
            RobotData = entity.RobotData,
            Line1Data = entity.Line1Data,
            Line2Data = entity.Line2Data,
            ChangeAction = action,
            IsEnabled = entity.IsEnabled,
            PerformedByUsername = username,
            PerformedAtUtc = DateTimeOffset.UtcNow
        };
    }

    private async Task ValidateUniqueNameAsync(int machineId, string modelName, int? existingId, CancellationToken cancellationToken)
    {
        if (await _dbContext.ModelProfiles.AnyAsync(
                x => x.MachineId == machineId
                     && x.ModelName == modelName
                     && !x.IsDeleted
                     && (!existingId.HasValue || x.Id != existingId.Value),
                cancellationToken))
        {
            throw new ValidationProblemException(new Dictionary<string, string[]>
            {
                ["modelName"] = ["A model with this name already exists for this machine."]
            });
        }
    }

    private static string SerializeData(Dictionary<string, object?> data)
    {
        return JsonSerializer.Serialize(data, JsonOptions);
    }

    private static Dictionary<string, object?> MergeData(string existingJson, Dictionary<string, object?> updates)
    {
        var merged = DeserializeData(existingJson);
        foreach (var item in updates)
        {
            merged[item.Key] = item.Value;
        }

        return merged;
    }

    private static bool DataEquals(string existingJson, Dictionary<string, object?> nextData)
    {
        var existingData = DeserializeData(existingJson);
        if (existingData.Count != nextData.Count)
        {
            return false;
        }

        foreach (var item in existingData)
        {
            if (!nextData.TryGetValue(item.Key, out var nextValue))
            {
                return false;
            }

            if (!ValueEquals(item.Value, nextValue))
            {
                return false;
            }
        }

        return true;
    }

    private static bool ValueEquals(object? left, object? right)
    {
        return NormalizeValue(left) == NormalizeValue(right);
    }

    private static bool HasPositiveValue(object? value)
    {
        return TryConvertToDouble(value, out var numericValue) && numericValue > 0d;
    }

    private static bool TryConvertToDouble(object? value, out double numericValue)
    {
        switch (value)
        {
            case null:
                numericValue = 0d;
                return false;
            case JsonElement element:
                if (element.ValueKind == JsonValueKind.Number && element.TryGetDouble(out numericValue))
                {
                    return true;
                }

                if (element.ValueKind == JsonValueKind.String
                    && double.TryParse(element.GetString(), NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out numericValue))
                {
                    return true;
                }

                numericValue = 0d;
                return false;
            case string text:
                return double.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out numericValue)
                    || double.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out numericValue);
            case IConvertible convertible:
                try
                {
                    numericValue = convertible.ToDouble(CultureInfo.InvariantCulture);
                    return true;
                }
                catch
                {
                    numericValue = 0d;
                    return false;
                }
            default:
                numericValue = 0d;
                return false;
        }
    }

    private static string NormalizeValue(object? value)
    {
        return value switch
        {
            null => "null:",
            JsonElement element => NormalizeJsonElement(element),
            bool b => $"bool:{b}",
            string s => $"string:{s}",
            double d => $"number:{Convert.ToDecimal(d).ToString("G29", CultureInfo.InvariantCulture)}",
            float f => $"number:{Convert.ToDecimal(f).ToString("G29", CultureInfo.InvariantCulture)}",
            decimal d => $"number:{d.ToString("G29", CultureInfo.InvariantCulture)}",
            int i => $"number:{i.ToString(CultureInfo.InvariantCulture)}",
            long l => $"number:{l.ToString(CultureInfo.InvariantCulture)}",
            _ => $"string:{value}"
        };
    }

    private static string NormalizeJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Null or JsonValueKind.Undefined => "null:",
            JsonValueKind.True => "bool:true",
            JsonValueKind.False => "bool:false",
            JsonValueKind.String => $"string:{element.GetString()}",
            JsonValueKind.Number when element.TryGetDecimal(out var number) =>
                $"number:{number.ToString("G29", CultureInfo.InvariantCulture)}",
            JsonValueKind.Number => $"number:{element.GetRawText()}",
            _ => $"json:{element.GetRawText()}"
        };
    }

    // ───── Excel helpers ─────

    private static readonly (string Key, string Label)[] RobotFieldDefinitions =
    [
        // Cải tiến theo yêu cầu mr.Tùng ngày 27/04/2026: Ẩn các điểm check gốc robot khỏi Excel import/export.
        // ("originCheck1X", "Tọa độ X Gốc check1"),
        // ("originCheck1Y", "Tọa độ Y Gốc check1"),
        // ("originCheck1Z", "Tọa độ Z Gốc check1"),
        // ("originCheck2X", "Tọa độ X Gốc check2"),
        // ("originCheck2Y", "Tọa độ Y Gốc check2"),
        // ("originCheck2Z", "Tọa độ Z Gốc check2"),
        ("jigProductHeight", "Độ cao trên Jig"),
        ("jigCenterOffset", "Ofset Tâm Jig"),
        ("jigDepthOffset", "Ofset độ cao âm xuống Jig"),
        ("modelJigClampType", "Model Jig tay kẹp"),
    ];

    private static readonly (string Key, string Label)[] Line1FieldDefinitions =
    [
        ("jigType", "Loại tay kẹp"),
        ("pickInputX", "Tọa độ X gắp SP đầu vào line"),
        ("pickInputZ", "Tọa độ Z gắp SP đầu vào line"),
        ("pickOp1X", "Tọa độ X an toàn lên xuống Op1"),
        ("pickOp1Z", "Tọa độ Z an toàn lên xuống Op1"),
        ("pickOp2X", "Tọa độ X an toàn lên xuống Op2"),
        ("pickOp2Z", "Tọa độ Z an toàn lên xuống Op2"),
        ("placeOp1X", "Tọa độ X chống tâm Op1"),
        ("placeOp1Z", "Tọa độ Z chống tâm Op1"),
        ("placeOp2X", "Tọa độ X chống tâm Op2"),
        ("placeOp2Z", "Tọa độ Z chống tâm Op2"),
        ("placeMeasureX", "Tọa độ X chống tâm máy đo"),
        ("placeMeasureZ", "Tọa độ Z chống tâm máy đo"),
        ("jigProductHeight", "Tọa độ Jig đỡ trục đầu vào"),
        ("grindingTimeOp1", "Thời gian mài Op1"),
        ("grindingTimeOp2", "Thời gian mài Op2"),
    ];

    private static readonly (string Key, string Label)[] Line2FieldDefinitions =
    [
        ("jigType", "Loại tay kẹp"),
        ("pickInputX", "Tọa độ X gắp SP đầu vào line"),
        ("pickInputZ", "Tọa độ Z gắp SP đầu vào line"),
        ("pickOp1X", "Tọa độ X an toàn lên xuống Op1"),
        ("pickOp1Z", "Tọa độ Z an toàn lên xuống Op1"),
        ("pickOp2X", "Tọa độ X an toàn lên xuống Op2"),
        ("pickOp2Z", "Tọa độ Z an toàn lên xuống Op2"),
        ("placeOp1X", "Tọa độ X chống tâm Op1"),
        ("placeOp1Z", "Tọa độ Z chống tâm Op1"),
        ("placeOp2X", "Tọa độ X chống tâm Op2"),
        ("placeOp2Z", "Tọa độ Z chống tâm Op2"),
        ("placeMeasureX", "Tọa độ X chống tâm máy đo"),
        ("placeMeasureZ", "Tọa độ Z chống tâm máy đo"),
        ("jigProductHeight", "Tọa độ Jig đỡ trục đầu vào"),
        ("grindingTimeOp1", "Thời gian mài Op1"),
        ("grindingTimeOp2", "Thời gian mài Op2"),
    ];

    private static void WriteModelsSheet(XLWorkbook workbook, IReadOnlyList<ModelProfileEntity> profiles)
    {
        var ws = workbook.Worksheets.Add(ModelsSheetName);
        var robotStart = ModelsRobotStartColumn;
        var line1Start = robotStart + RobotFieldDefinitions.Length;
        var line2Start = line1Start + Line1FieldDefinitions.Length;
        var lastColumn = line2Start + Line2FieldDefinitions.Length - 1;

        WriteGroupHeader(ws, 1, ModelsMetadataColumnCount, "Thông tin chung");
        WriteGroupHeader(ws, robotStart, line1Start - 1, "Robot");
        WriteGroupHeader(ws, line1Start, line2Start - 1, "Line 1");
        WriteGroupHeader(ws, line2Start, lastColumn, "Line 2");

        ws.Cell(ModelsHeaderRow, 1).Value = "Article ID";
        foreach (var metadataField in MetadataFieldDefinitions)
        {
            ws.Cell(ModelsHeaderRow, metadataField.Column).Value = FormatExcelHeaderLabel(metadataField.Label);
        }

        WriteFieldHeaders(ws, robotStart, RobotFieldDefinitions);
        WriteFieldHeaders(ws, line1Start, Line1FieldDefinitions);
        WriteFieldHeaders(ws, line2Start, Line2FieldDefinitions);

        var headerRange = ws.Range(1, 1, ModelsHeaderRow, lastColumn);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#70AD47");
        headerRange.Style.Alignment.WrapText = true;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        ws.Row(1).Height = ExcelGroupHeaderRowHeight;
        ws.Row(ModelsHeaderRow).Height = ExcelDetailHeaderRowHeight;

        for (var row = 0; row < profiles.Count; row++)
        {
            var profile = profiles[row];
            var excelRow = row + ModelsDataStartRow;

            ws.Cell(excelRow, 1).Value = profile.ModelName;
            WriteMetadataCells(ws, excelRow, profile);
            WriteFieldValues(ws, excelRow, robotStart, RobotFieldDefinitions, DeserializeData(profile.RobotData));
            WriteFieldValues(ws, excelRow, line1Start, Line1FieldDefinitions, DeserializeData(profile.Line1Data));
            WriteFieldValues(ws, excelRow, line2Start, Line2FieldDefinitions, DeserializeData(profile.Line2Data));
        }

        ws.SheetView.FreezeRows(ModelsHeaderRow);
        ws.SheetView.FreezeColumns(1);
        var tableLastRow = Math.Max(ModelsHeaderRow, profiles.Count + ModelsHeaderRow);
        ws.Range(ModelsHeaderRow, 1, tableLastRow, lastColumn).SetAutoFilter();
        ws.Columns().AdjustToContents();
        ApplyExcelColumnWidths(ws, robotStart, lastColumn);
        ApplyExcelTableBorders(ws, tableLastRow, lastColumn);
    }

    private static void WriteGroupHeader(IXLWorksheet ws, int startColumn, int endColumn, string title)
    {
        var range = ws.Range(1, startColumn, 1, endColumn);
        range.Merge();
        ws.Cell(1, startColumn).Value = title;
    }

    private static void WriteFieldHeaders(IXLWorksheet ws, int startColumn, (string Key, string Label)[] fieldDefs)
    {
        for (var i = 0; i < fieldDefs.Length; i++)
        {
            ws.Cell(ModelsHeaderRow, startColumn + i).Value = FormatExcelHeaderLabel(fieldDefs[i].Label);
        }
    }

    private static void WriteFieldValues(
        IXLWorksheet ws,
        int row,
        int startColumn,
        (string Key, string Label)[] fieldDefs,
        Dictionary<string, object?> data)
    {
        for (var col = 0; col < fieldDefs.Length; col++)
        {
            if (data.TryGetValue(fieldDefs[col].Key, out var value) && value is not null)
            {
                ws.Cell(row, startColumn + col).SetValue(ConvertToXlValue(value));
            }
        }
    }

    private static string FormatExcelHeaderLabel(string label)
    {
        if (string.IsNullOrWhiteSpace(label) || label.Contains('\n'))
        {
            return label;
        }

        var fixedLabel = label switch
        {
            "Chương trình gia công" => "Chương trình\ngia công",
            "Đường kính ngoài trục" => "Đường kính ngoài\ntrục",
            "Lần sửa cuối" => "Lần sửa\ncuối",
            _ => label
        };

        if (!string.Equals(fixedLabel, label, StringComparison.Ordinal))
        {
            return fixedLabel;
        }

        if (label.Length <= ExcelHeaderWrapThreshold)
        {
            return label;
        }

        var lineIndex = label.LastIndexOf(" Line ", StringComparison.OrdinalIgnoreCase);
        if (lineIndex > 0 && lineIndex < label.Length - 6)
        {
            return $"{label[..lineIndex]}\n{label[(lineIndex + 1)..]}";
        }

        var splitIndex = FindHeaderSplitIndex(label);
        return splitIndex > 0
            ? $"{label[..splitIndex]}\n{label[(splitIndex + 1)..]}"
            : label;
    }

    private static int FindHeaderSplitIndex(string label)
    {
        var target = label.Length / 2;
        var bestIndex = -1;
        var bestDistance = int.MaxValue;

        for (var i = 0; i < label.Length; i++)
        {
            if (label[i] != ' ')
            {
                continue;
            }

            if (i < 6 || label.Length - i < 6)
            {
                continue;
            }

            var distance = Math.Abs(i - target);
            if (distance < bestDistance)
            {
                bestIndex = i;
                bestDistance = distance;
            }
        }

        return bestIndex;
    }

    private static void ApplyExcelColumnWidths(IXLWorksheet ws, int parameterStartColumn, int lastColumn)
    {
        ws.Column(1).Width = Math.Min(Math.Max(ws.Column(1).Width, 11), 13);
        ws.Column(2).Width = Math.Min(Math.Max(ws.Column(2).Width, 9), 11);
        ws.Column(3).Width = 11;
        ws.Column(4).Width = 11;
        ws.Column(5).Width = 9;

        for (var col = parameterStartColumn; col <= lastColumn; col++)
        {
            ws.Column(col).Width = Math.Min(Math.Max(ws.Column(col).Width, ExcelMinParameterColumnWidth), ExcelMaxWrappedColumnWidth);
        }
    }

    private static void ApplyExcelTableBorders(IXLWorksheet ws, int lastRow, int lastColumn)
    {
        var tableRange = ws.Range(1, 1, lastRow, lastColumn);
        tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        tableRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
        tableRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
        tableRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
        tableRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        tableRange.Style.Border.OutsideBorderColor = XLColor.Black;
        tableRange.Style.Border.InsideBorderColor = XLColor.Black;
        tableRange.Style.Border.LeftBorderColor = XLColor.Black;
        tableRange.Style.Border.RightBorderColor = XLColor.Black;
        tableRange.Style.Border.TopBorderColor = XLColor.Black;
        tableRange.Style.Border.BottomBorderColor = XLColor.Black;
    }

    private static ExcelModelRows ReadModelsSheet(XLWorkbook workbook, List<ModelExcelValidationError> errors)
    {
        var result = new ExcelModelRows();

        if (!workbook.TryGetWorksheet(ModelsSheetName, out var ws))
        {
            errors.Add(new ModelExcelValidationError
            {
                Sheet = ModelsSheetName,
                Field = "file",
                Message = "File Excel phải có sheet Models theo format mới. Vui lòng export file mẫu mới rồi nhập lại."
            });
            return result;
        }

        var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
        var robotStart = ModelsRobotStartColumn;
        var line1Start = robotStart + RobotFieldDefinitions.Length;
        var line2Start = line1Start + Line1FieldDefinitions.Length;
        var lastColumn = line2Start + Line2FieldDefinitions.Length - 1;

        ValidateModelsSheetHeader(ws, errors);
        if (errors.Count > 0)
        {
            return result;
        }

        for (var row = ModelsDataStartRow; row <= lastRow; row++)
        {
            if (!HasAnyRowValue(ws, row, lastColumn))
            {
                continue;
            }

            var modelNameCell = ws.Cell(row, 1);
            var modelName = modelNameCell.GetString().Trim();
            if (string.IsNullOrWhiteSpace(modelName))
            {
                errors.Add(new ModelExcelValidationError
                {
                    Sheet = ModelsSheetName,
                    Row = row,
                    Column = "A",
                    Field = "Article ID",
                    Value = modelNameCell.GetString(),
                    Message = $"Sheet {ModelsSheetName}, dòng {row}, cột A: thiếu Article ID."
                });
                continue;
            }

            if (result.Rows.ContainsKey(modelName))
            {
                errors.Add(new ModelExcelValidationError
                {
                    Sheet = ModelsSheetName,
                    Row = row,
                    Column = "A",
                    ArticleId = modelName,
                    Field = "Article ID",
                    Value = modelName,
                    Message = $"Sheet {ModelsSheetName}, dòng {row}: Article ID '{modelName}' bị trùng trong cùng sheet."
                });
                continue;
            }

            var metadata = new Dictionary<string, ExcelMetadataValue>(StringComparer.Ordinal);
            foreach (var field in MetadataFieldDefinitions)
            {
                var cell = ws.Cell(row, field.Column);
                if (cell.IsEmpty())
                {
                    continue;
                }

                var parsed = TryReadMetadataValue(ModelsSheetName, row, modelName, cell, field, errors);
                if (parsed is not null)
                {
                    metadata[field.Key] = parsed;
                }
            }

            var robotData = ReadFieldValues(ws, row, robotStart, RobotFieldDefinitions);
            var line1Data = ReadFieldValues(ws, row, line1Start, Line1FieldDefinitions);
            var line2Data = ReadFieldValues(ws, row, line2Start, Line2FieldDefinitions);

            result.Rows[modelName] = new ExcelModelRow(modelName, row, metadata, robotData, line1Data, line2Data);
        }

        return result;
    }

    private static Dictionary<string, object?> ReadFieldValues(
        IXLWorksheet ws,
        int row,
        int startColumn,
        (string Key, string Label)[] fieldDefs)
    {
        var data = new Dictionary<string, object?>();

        for (var col = 0; col < fieldDefs.Length; col++)
        {
            var cell = ws.Cell(row, startColumn + col);
            if (!cell.IsEmpty())
            {
                data[fieldDefs[col].Key] = cell.Value.IsNumber
                    ? cell.GetDouble()
                    : cell.GetString();
            }
        }

        return data;
    }

    private static ModelExcelValidationError CreateActivationExcelValidationError(
        ExcelModelRow row,
        IReadOnlyList<string> missingActivationFields)
    {
        return new ModelExcelValidationError
        {
            Sheet = ModelsSheetName,
            Row = row.Row,
            Column = "-",
            ArticleId = row.ArticleId,
            Field = "Active",
            Message = $"Model đang Active nên phải có dữ liệu > 0 cho: {string.Join(", ", missingActivationFields)}."
        };
    }

    private static void WriteMetadataCells(IXLWorksheet ws, int row, ModelProfileEntity profile)
    {
        ws.Cell(row, 2).Value = profile.ItemType ?? string.Empty;
        ws.Cell(row, 3).Value = DefaultInt(profile.MachiningProgram);
        ws.Cell(row, 4).Value = Convert.ToDouble(DefaultDecimal(profile.OuterShaftDiameter));
        ws.Cell(row, 5).Value = DefaultFloat(profile.DiameterOp1);
        ws.Cell(row, 6).Value = DefaultFloat(profile.DiameterOp2);
        ws.Cell(row, 7).Value = DefaultTrayType(profile.TrayType);
        ws.Cell(row, 8).Value = DefaultOrderInput(profile.OrderInput);
    }

    private static void ValidateModelsSheetHeader(IXLWorksheet ws, List<ModelExcelValidationError> errors)
    {
        var columnEHeader = NormalizeHeaderText(ws.Cell(ModelsHeaderRow, 5).GetString());
        if (string.Equals(columnEHeader, NormalizeHeaderText(LegacyTrayUsageHeader), StringComparison.OrdinalIgnoreCase))
        {
            errors.Add(new ModelExcelValidationError
            {
                Sheet = ModelsSheetName,
                Row = ModelsHeaderRow,
                Column = "E",
                Field = LegacyTrayUsageHeader,
                Value = ws.Cell(ModelsHeaderRow, 5).GetString(),
                Message = "File Excel đang dùng mẫu cũ có cột Tray sử dụng. Vui lòng export file mẫu mới rồi nhập lại."
            });
            return;
        }

        var columnHHeader = NormalizeHeaderText(ws.Cell(ModelsHeaderRow, 8).GetString());
        if (string.Equals(columnHHeader, NormalizeHeaderText(ExpectedOrderInputHeader), StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        errors.Add(new ModelExcelValidationError
        {
            Sheet = ModelsSheetName,
            Row = ModelsHeaderRow,
            Column = "H",
            Field = ExpectedOrderInputHeader,
            Value = ws.Cell(ModelsHeaderRow, 8).GetString(),
            Message = "File Excel đang dùng mẫu cũ thiếu cột Đường kính Op1/Op2 trong thông tin chung. Vui lòng export file mẫu mới rồi nhập lại."
        });
    }

    private static string NormalizeHeaderText(string value)
    {
        return new string((value ?? string.Empty).Where(c => !char.IsWhiteSpace(c)).ToArray());
    }

    private static bool HasAnyRowValue(IXLWorksheet ws, int row, int lastColumn)
    {
        for (var col = 1; col <= lastColumn; col++)
        {
            if (!ws.Cell(row, col).IsEmpty())
            {
                return true;
            }
        }

        return false;
    }

    private static ExcelMetadataValue? TryReadMetadataValue(
        string sheetName,
        int row,
        string articleId,
        IXLCell cell,
        ExcelMetadataField field,
        List<ModelExcelValidationError> errors)
    {
        var rawValue = GetCellDisplayValue(cell);
        var column = ColumnLetter(field.Column);
        var trimmed = rawValue.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return null;
        }

        object? value;
        string normalized;
        switch (field.Kind)
        {
            case ExcelMetadataKind.Text:
                value = trimmed;
                normalized = $"text:{trimmed}";
                break;
            case ExcelMetadataKind.Int:
                if (!TryReadInt(cell, trimmed, out var intValue))
                {
                    errors.Add(CreateCellError(sheetName, row, column, articleId, field.Label, rawValue, $"{field.Label} phải là số nguyên."));
                    return null;
                }
                value = intValue;
                normalized = intValue.ToString(CultureInfo.InvariantCulture);
                break;
            case ExcelMetadataKind.Decimal:
                if (!TryReadDecimal(cell, trimmed, out var decimalValue))
                {
                    errors.Add(CreateCellError(sheetName, row, column, articleId, field.Label, rawValue, $"{field.Label} phải là số."));
                    return null;
                }
                value = decimalValue;
                normalized = decimalValue.ToString("G29", CultureInfo.InvariantCulture);
                break;
            case ExcelMetadataKind.Float:
                if (!TryReadFloat(cell, trimmed, out var floatValue))
                {
                    errors.Add(CreateCellError(sheetName, row, column, articleId, field.Label, rawValue, $"{field.Label} phải là số."));
                    return null;
                }
                value = floatValue;
                normalized = floatValue.ToString("G9", CultureInfo.InvariantCulture);
                break;
            case ExcelMetadataKind.TrayType:
                if (!TryReadTrayType(cell, trimmed, out var trayType))
                {
                    errors.Add(CreateCellError(sheetName, row, column, articleId, field.Label, rawValue, "Loại tray chỉ chấp nhận Nhỏ, To, 0 hoặc 1."));
                    return null;
                }
                value = trayType;
                normalized = trayType.ToString(CultureInfo.InvariantCulture);
                break;
            case ExcelMetadataKind.OrderInput:
                if (!TryReadOrderInput(cell, trimmed, out var orderInput))
                {
                    errors.Add(CreateCellError(sheetName, row, column, articleId, field.Label, rawValue, "Nhập order chỉ chấp nhận 0 hoặc 1."));
                    return null;
                }
                value = orderInput;
                normalized = orderInput.ToString(CultureInfo.InvariantCulture);
                break;
            default:
                return null;
        }

        return new ExcelMetadataValue(field, sheetName, row, column, rawValue, normalized, value);
    }

    private static ModelExcelValidationError CreateCellError(
        string sheetName,
        int row,
        string column,
        string articleId,
        string field,
        string value,
        string message)
    {
        return new ModelExcelValidationError
        {
            Sheet = sheetName,
            Row = row,
            Column = column,
            ArticleId = articleId,
            Field = field,
            Value = value,
            Message = $"Sheet {sheetName}, dòng {row}, cột {column}: {message}"
        };
    }

    private static bool TryReadInt(IXLCell cell, string text, out int value)
    {
        if (cell.Value.IsNumber)
        {
            var number = cell.GetDouble();
            if (Math.Abs(number % 1) < 0.0000001 && number >= int.MinValue && number <= int.MaxValue)
            {
                value = Convert.ToInt32(number);
                return true;
            }

            value = default;
            return false;
        }

        return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }

    private static bool TryReadDecimal(IXLCell cell, string text, out decimal value)
    {
        if (cell.Value.IsNumber)
        {
            value = Convert.ToDecimal(cell.GetDouble());
            return true;
        }

        return decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out value)
            || decimal.TryParse(text, NumberStyles.Number, CultureInfo.GetCultureInfo("vi-VN"), out value);
    }

    private static bool TryReadFloat(IXLCell cell, string text, out float value)
    {
        if (cell.Value.IsNumber)
        {
            value = (float)cell.GetDouble();
            return true;
        }

        return float.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out value)
            || float.TryParse(text, NumberStyles.Number, CultureInfo.GetCultureInfo("vi-VN"), out value);
    }

    private static bool TryReadTrayType(IXLCell cell, string text, out int value)
    {
        if (TryReadInt(cell, text, out var intValue) && intValue is 0 or 1)
        {
            value = intValue;
            return true;
        }

        if (string.Equals(text, "Nho", StringComparison.OrdinalIgnoreCase)
            || string.Equals(text, "Nhỏ", StringComparison.OrdinalIgnoreCase))
        {
            value = 0;
            return true;
        }

        if (string.Equals(text, "To", StringComparison.OrdinalIgnoreCase))
        {
            value = 1;
            return true;
        }

        value = default;
        return false;
    }

    private static bool TryReadOrderInput(IXLCell cell, string text, out int value)
    {
        if (TryReadInt(cell, text, out var intValue) && intValue is 0 or 1)
        {
            value = intValue;
            return true;
        }

        value = default;
        return false;
    }

    private static string GetCellDisplayValue(IXLCell cell)
    {
        if (cell.Value.IsNumber)
        {
            return cell.GetDouble().ToString("G29", CultureInfo.InvariantCulture);
        }

        return cell.GetString();
    }

    private static string ColumnLetter(int columnNumber)
    {
        var dividend = columnNumber;
        var columnName = string.Empty;
        while (dividend > 0)
        {
            var modulo = (dividend - 1) % 26;
            columnName = Convert.ToChar('A' + modulo) + columnName;
            dividend = (dividend - modulo) / 26;
        }

        return columnName;
    }

    private static Dictionary<string, object?> DeserializeData(string json)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "{}")
        {
            return new Dictionary<string, object?>();
        }

        return JsonSerializer.Deserialize<Dictionary<string, object?>>(json, JsonOptions)
               ?? new Dictionary<string, object?>();
    }

    private static XLCellValue ConvertToXlValue(object value)
    {
        return value switch
        {
            JsonElement { ValueKind: JsonValueKind.Number } je => je.GetDouble(),
            JsonElement { ValueKind: JsonValueKind.String } je => je.GetString() ?? string.Empty,
            JsonElement je => je.ToString(),
            double d => d,
            float f => (double)f,
            decimal d => Convert.ToDouble(d),
            int i => i,
            long l => l,
            string s => s,
            _ => value.ToString() ?? string.Empty
        };
    }

    private enum ExcelMetadataKind
    {
        Text,
        Int,
        Decimal,
        Float,
        TrayType,
        OrderInput
    }

    private sealed record ExcelMetadataField(string Key, string Label, int Column, ExcelMetadataKind Kind);

    private sealed record ExcelMetadataValue(
        ExcelMetadataField Field,
        string Sheet,
        int Row,
        string Column,
        string RawValue,
        string NormalizedValue,
        object? Value);

    private sealed record ExcelModelRow(
        string ArticleId,
        int Row,
        Dictionary<string, ExcelMetadataValue> Metadata,
        Dictionary<string, object?> RobotData,
        Dictionary<string, object?> Line1Data,
        Dictionary<string, object?> Line2Data);

    private sealed class ExcelModelRows
    {
        public Dictionary<string, ExcelModelRow> Rows { get; } = new(StringComparer.OrdinalIgnoreCase);
    }

    private sealed class IncomingModelMetadata
    {
        public string? ItemType { get; set; }
        public bool HasItemType { get; set; }
        public int? MachiningProgram { get; set; }
        public bool HasMachiningProgram { get; set; }
        public string? Spare1 { get; set; }
        public bool HasSpare1 { get; set; }
        public string? Spare2 { get; set; }
        public bool HasSpare2 { get; set; }
        public decimal? OuterShaftDiameter { get; set; }
        public bool HasOuterShaftDiameter { get; set; }
        public float? DiameterOp1 { get; set; }
        public bool HasDiameterOp1 { get; set; }
        public float? DiameterOp2 { get; set; }
        public bool HasDiameterOp2 { get; set; }
        public int? TrayUsage { get; set; }
        public bool HasTrayUsage { get; set; }
        public int? TrayType { get; set; }
        public bool HasTrayType { get; set; }
        public int? OrderInput { get; set; }
        public bool HasOrderInput { get; set; }
    }
}
