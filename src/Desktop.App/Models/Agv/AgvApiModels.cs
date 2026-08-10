using System.Text.Json.Serialization;

namespace Desktop.App.Models.Agv;

public class CreateCommandRequest
{
    [JsonPropertyName("machineCode")]
    public string MachineCode { get; set; } = string.Empty;

    [JsonPropertyName("position")]
    public int Position { get; set; }
}

public class CheckStatusRequest
{
    [JsonPropertyName("machineCode")]
    public string MachineCode { get; set; } = string.Empty;

    [JsonPropertyName("position")]
    public int Position { get; set; }
}

public class ConfirmCommandRequest
{
    [JsonPropertyName("machineCode")]
    public string MachineCode { get; set; } = string.Empty;

    [JsonPropertyName("position")]
    public int Position { get; set; }
}

public class AgvApiResponse<T>
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public T? Data { get; set; }
}

public class AgvStatusPayload
{
    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("declarationId")]
    public int? DeclarationId { get; set; }
}

public class MachineLoadInfo
{
    [JsonPropertyName("declarationId")]
    public int DeclarationId { get; set; }

    [JsonPropertyName("machineSlotIndex")]
    public int MachineSlotIndex { get; set; }

    [JsonPropertyName("shelfLayoutType")]
    public int ShelfLayoutType { get; set; }

    [JsonPropertyName("ordersJson")]
    public string OrdersJson { get; set; } = "[]";

    [JsonPropertyName("orderCount")]
    public int OrderCount { get; set; }
}

public class AgvOrderData
{
    [JsonPropertyName("orderId")]
    public string? OrderId { get; set; }

    [JsonPropertyName("modelId")]
    public string? ModelId { get; set; }

    [JsonPropertyName("modelName")]
    public string ModelName { get; set; } = string.Empty;

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("startPosition")]
    public int StartPosition { get; set; }

    [JsonPropertyName("trayIndex")]
    public int TrayIndex { get; set; }

    [JsonPropertyName("trayType")]
    public int TrayType { get; set; }

    [JsonPropertyName("orderSequence")]
    public int OrderSequence { get; set; }

    [JsonPropertyName("jigType")]
    public int JigType { get; set; }

    [JsonPropertyName("partHoverHeight")]
    public float? PartHoverHeight { get; set; }

    [JsonPropertyName("jigCenterOffset")]
    public float? JigCenterOffset { get; set; }

    [JsonPropertyName("jigDepthOffset")]
    public float? JigDepthOffset { get; set; }

    [JsonPropertyName("diameterOp1")]
    public float? DiameterOp1 { get; set; }

    [JsonPropertyName("inputBlankDiameter")]
    public float? InputBlankDiameter { get; set; }

    [JsonPropertyName("op2ChuckSleeveDepth")]
    public float? Op2ChuckSleeveDepth { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("completedAtUtc")]
    public DateTimeOffset? CompletedAtUtc { get; set; }
}

public class AgvCallEligibilityInfo
{
    [JsonPropertyName("machineId")]
    public int? MachineId { get; set; }

    [JsonPropertyName("machineCode")]
    public string MachineCode { get; set; } = string.Empty;

    [JsonPropertyName("machineName")]
    public string? MachineName { get; set; }

    [JsonPropertyName("machineSlotIndex")]
    public int MachineSlotIndex { get; set; }

    [JsonPropertyName("stagingSlotIndex")]
    public int? StagingSlotIndex { get; set; }

    [JsonPropertyName("hasActiveDeclaration")]
    public bool HasActiveDeclaration { get; set; }

    [JsonPropertyName("declarationId")]
    public int? DeclarationId { get; set; }

    [JsonPropertyName("declarationStatus")]
    public string? DeclarationStatus { get; set; }

    [JsonPropertyName("orderCount")]
    public int? OrderCount { get; set; }

    [JsonPropertyName("reasonCode")]
    public string ReasonCode { get; set; } = string.Empty;

    [JsonPropertyName("reasonMessage")]
    public string ReasonMessage { get; set; } = string.Empty;
}
