namespace Server.Api.Contracts.Responses;

public sealed class ProductionLifecycleReportResponse
{
    public DateTimeOffset GeneratedAtUtc { get; set; }
    public int? MachineId { get; set; }
    public DateTimeOffset? FromUtc { get; set; }
    public DateTimeOffset? ToUtc { get; set; }
    public string? Status { get; set; }
    public int? ShelfIndex { get; set; }
    public int TotalDeclarations { get; set; }
    public int InProgressDeclarations { get; set; }
    public int CompletedDeclarations { get; set; }
    public int FailedDeclarations { get; set; }
    public IReadOnlyList<ProductionLifecycleDeclarationResponse> Items { get; set; } = [];
}

public sealed class ProductionLifecycleDeclarationResponse
{
    public int DeclarationId { get; set; }
    public int MachineId { get; set; }
    public string MachineCode { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public string Mode { get; set; } = string.Empty;
    public int? ShelfIndex { get; set; }
    public int? StagingSlotIndex { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsLoadingParameters { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? AgvTakenAtUtc { get; set; }
    public DateTimeOffset? LoadedAtUtc { get; set; }
    public DateTimeOffset? ProductionStartedAtUtc { get; set; }
    public DateTimeOffset? CompletedAtUtc { get; set; }
    public DateTimeOffset? ClearedAtUtc { get; set; }
    public DateTimeOffset? CancelledAtUtc { get; set; }
    public int? AgvPickupDurationSeconds { get; set; }
    public int? ProductionDurationSeconds { get; set; }
    public int? LifecycleDurationSeconds { get; set; }
    public IReadOnlyList<ProductionLifecycleOrderResponse> Orders { get; set; } = [];
}

public sealed class ProductionLifecycleOrderResponse
{
    public string OrderId { get; set; } = string.Empty;
    public int OrderSequence { get; set; }
    public string ModelName { get; set; } = string.Empty;
    public string ReportModelName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int TrayIndex { get; set; }
    public int TrayType { get; set; }
    public int JigType { get; set; }
    public string Status { get; set; } = string.Empty;
}
