using System.Text.Json;
using Desktop.App.Configuration;
using Desktop.App.Models.Agv;
using Desktop.App.Services.Api;
using Shared.Models.ModelProfiles;

namespace Desktop.App.Services.Robot;

internal sealed class RobotJigTypeResolver
{
    private IModelProfileApiClient? _modelProfileApiClient;

    public void AttachModelProfileApiClient(IModelProfileApiClient modelProfileApiClient)
    {
        _modelProfileApiClient = modelProfileApiClient;
    }

    public async Task<int> ResolveAsync(AgvPosition position, string? modelId, string? modelName)
    {
        var data = await ResolveProfileLineDataAsync(position, modelId, modelName);
        return data.JigType;
    }

    public async Task<RobotProfileLineData> ResolveProfileLineDataAsync(AgvPosition position, string? modelId, string? modelName)
    {
        var modelProfileApiClient = _modelProfileApiClient;
        if (modelProfileApiClient is null)
        {
            return RobotProfileLineData.Empty;
        }

        if (string.IsNullOrWhiteSpace(modelId) && string.IsNullOrWhiteSpace(modelName))
        {
            return RobotProfileLineData.Empty;
        }

        var machineId = await modelProfileApiClient.ResolveMachineIdAsync(AppSettings.Current.MachineCode);
        var profile = await ResolveCurrentProfileAsync(modelProfileApiClient, machineId, modelId, modelName);
        return profile is null
            ? RobotProfileLineData.Empty
            : BuildLineData(profile, isLine2: position == AgvPosition.Position2);
    }

    public async Task EnsureProfileEnabledForWriteAsync(string? modelId, string? modelName)
    {
        var profile = await ResolveCurrentProfileAsync(modelId, modelName);
        ThrowIfInactive(profile, modelId, modelName);
    }

    public async Task<RobotProfileLineData> ResolveProfileLineDataForWriteAsync(AgvPosition position, string? modelId, string? modelName)
    {
        var profile = await ResolveCurrentProfileAsync(modelId, modelName);
        if (profile is null)
        {
            return RobotProfileLineData.Empty;
        }

        ThrowIfInactive(profile, modelId, modelName);
        return BuildLineData(profile, isLine2: position == AgvPosition.Position2);
    }

    private async Task<ModelProfileDto?> ResolveCurrentProfileAsync(string? modelId, string? modelName)
    {
        var modelProfileApiClient = _modelProfileApiClient;
        if (modelProfileApiClient is null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(modelId) && string.IsNullOrWhiteSpace(modelName))
        {
            return null;
        }

        var machineId = await modelProfileApiClient.ResolveMachineIdAsync(AppSettings.Current.MachineCode);
        return await ResolveCurrentProfileAsync(modelProfileApiClient, machineId, modelId, modelName);
    }

    private static void ThrowIfInactive(ModelProfileDto? profile, string? modelId, string? modelName)
    {
        if (profile?.IsEnabled == false)
        {
            throw new InactiveModelProfileException(profile.ModelName, modelId, modelName);
        }
    }

    private static async Task<ModelProfileDto?> ResolveCurrentProfileAsync(
        IModelProfileApiClient modelProfileApiClient,
        int machineId,
        string? modelId,
        string? modelName)
    {
        ModelProfileDto? profile = null;
        var normalizedModelId = modelId?.Trim();
        var normalizedModelName = modelName?.Trim();

        if (int.TryParse(normalizedModelId, out var profileId) && profileId > 0)
        {
            profile = await modelProfileApiClient.GetByIdAsync(machineId, profileId);
            if (profile?.IsDeleted == true)
            {
                profile = null;
            }
        }

        if (profile is null && !string.IsNullOrWhiteSpace(normalizedModelName))
        {
            profile = await modelProfileApiClient.GetByNameAsync(machineId, normalizedModelName);
            if (profile?.IsDeleted == true)
            {
                profile = null;
            }
        }

        if (profile is null
            && !string.IsNullOrWhiteSpace(normalizedModelId)
            && !string.Equals(normalizedModelId, normalizedModelName, StringComparison.OrdinalIgnoreCase))
        {
            profile = await modelProfileApiClient.GetByNameAsync(machineId, normalizedModelId);
            if (profile?.IsDeleted == true)
            {
                profile = null;
            }
        }

        return profile;
    }

    private static RobotProfileLineData BuildLineData(ModelProfileDto profile, bool isLine2)
    {
        var lineData = isLine2 ? profile.Line2Data : profile.Line1Data;
        var robotData = profile.RobotData;

        var jigType = TryReadInt(robotData, "modelJigClampType", out var clampType)
            ? clampType
            : (TryReadInt(lineData, "jigType", out var resolvedJigType) ? resolvedJigType : 0);

        return new RobotProfileLineData
        {
            JigType = jigType,
            PartHoverHeight = ReadFloatOrDefault(robotData, "jigProductHeight"),
            JigCenterOffset = ReadFloatOrDefault(robotData, "jigCenterOffset"),
            JigDepthOffset = ReadFloatOrDefault(robotData, "jigDepthOffset"),
            DiameterOp1 = profile.DiameterOp1 ?? 0f
        };
    }

    private static float ReadFloatOrDefault(IReadOnlyDictionary<string, object?> data, string key)
    {
        return TryReadFloat(data, key, out var value) ? value : 0f;
    }

    private static bool TryReadInt(IReadOnlyDictionary<string, object?> data, string key, out int value)
    {
        value = 0;
        if (!data.TryGetValue(key, out var raw) || raw is null)
        {
            return false;
        }

        if (raw is int i)
        {
            value = i;
            return true;
        }

        if (raw is long l && l <= int.MaxValue && l >= int.MinValue)
        {
            value = (int)l;
            return true;
        }

        if (raw is JsonElement json)
        {
            if (json.ValueKind == JsonValueKind.Number && json.TryGetInt32(out var jsonInt))
            {
                value = jsonInt;
                return true;
            }

            if (json.ValueKind == JsonValueKind.String && int.TryParse(json.GetString(), out var jsonParsed))
            {
                value = jsonParsed;
                return true;
            }
        }

        if (int.TryParse(raw.ToString(), out var parsed))
        {
            value = parsed;
            return true;
        }

        return false;
    }

    private static bool TryReadFloat(IReadOnlyDictionary<string, object?> data, string key, out float value)
    {
        value = 0f;
        if (!data.TryGetValue(key, out var raw) || raw is null)
        {
            return false;
        }

        if (raw is float f)
        {
            value = f;
            return true;
        }

        if (raw is double d)
        {
            value = (float)d;
            return true;
        }

        if (raw is decimal dec)
        {
            value = (float)dec;
            return true;
        }

        if (raw is JsonElement json)
        {
            if (json.ValueKind == JsonValueKind.Number && json.TryGetSingle(out var jsonFloat))
            {
                value = jsonFloat;
                return true;
            }

            if (json.ValueKind == JsonValueKind.String
                && float.TryParse(json.GetString(), out var parsedJson))
            {
                value = parsedJson;
                return true;
            }
        }

        if (float.TryParse(raw.ToString(), out var parsed))
        {
            value = parsed;
            return true;
        }

        return false;
    }
}

internal sealed class InactiveModelProfileException : InvalidOperationException
{
    public InactiveModelProfileException(string modelName, string? modelId, string? requestedModelName)
        : base($"Model '{modelName}' dang Deactive. Vui long Active model truoc khi ghi order xuong PLC.")
    {
        ModelName = modelName;
        ModelId = modelId;
        RequestedModelName = requestedModelName;
    }

    public string ModelName { get; }
    public string? ModelId { get; }
    public string? RequestedModelName { get; }
}

internal sealed record RobotProfileLineData
{
    public static RobotProfileLineData Empty { get; } = new();

    public int JigType { get; init; }
    public float CheckPoint1X { get; init; }
    public float CheckPoint1Y { get; init; }
    public float CheckPoint1Z { get; init; }
    public float PartHoverHeight { get; init; }
    public float JigCenterOffset { get; init; }
    public float JigDepthOffset { get; init; }
    public float DiameterOp1 { get; init; }
}
