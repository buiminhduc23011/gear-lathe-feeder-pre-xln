namespace Desktop.App.Models.Reports;

public sealed class ProductionLifecycleReportDto
{
    public DateTimeOffset GeneratedAtUtc { get; set; }
    public IReadOnlyList<ProductionLifecycleDeclarationDto> Items { get; set; } = [];
    public int TotalDeclarations { get; set; }
    public int InProgressDeclarations { get; set; }
    public int CompletedDeclarations { get; set; }
    public int FailedDeclarations { get; set; }
}

public sealed class ProductionLifecycleDeclarationDto
{
    public int DeclarationId { get; set; }
    public string MachineCode { get; set; } = string.Empty;
    public int? ShelfIndex { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsLoadingParameters { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? AgvTakenAtUtc { get; set; }
    public DateTimeOffset? ProductionStartedAtUtc { get; set; }
    public DateTimeOffset? CompletedAtUtc { get; set; }
    public int? AgvPickupDurationSeconds { get; set; }
    public int? ProductionDurationSeconds { get; set; }
    public int? LifecycleDurationSeconds { get; set; }
    public IReadOnlyList<ProductionLifecycleOrderDto> Orders { get; set; } = [];

    public string StatusDisplay => ProductionLifecycleStatusMapper.ToVietnamese(Status);

    public string OrderSummary
    {
        get
        {
            if (Orders.Count == 0)
            {
                return "-";
            }

            return string.Join(" | ", Orders
                .OrderBy(x => x.OrderSequence)
                .Select(x =>
                    $"#{x.OrderSequence} Order:{(string.IsNullOrWhiteSpace(x.OrderId) ? "-" : x.OrderId)} Model:{x.DisplayModelName} Article:{(string.IsNullOrWhiteSpace(x.ModelName) ? "-" : x.ModelName)} Qty:{x.Quantity} Tray:{x.TrayIndex}/{x.TrayType} ({x.StatusDisplay})"));
        }
    }
}

public sealed class ProductionLifecycleOrderDto
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

    public string DisplayModelName => string.IsNullOrWhiteSpace(ReportModelName)
        ? (string.IsNullOrWhiteSpace(ModelName) ? "-" : ModelName)
        : ReportModelName.Trim();

    public string StatusDisplay => ProductionLifecycleStatusMapper.ToVietnamese(Status);
}

public static class ProductionLifecycleStatusMapper
{
    public static string ToVietnamese(string? status)
    {
        return status switch
        {
            "Created" => "Đã tạo",
            "AgvTaken" => "AGV đã lấy",
            "Loaded" => "Đã load",
            "InProduction" => "Đang sản xuất",
            "Completed" => "Hoàn thành",
            "Cleared" => "Đã hủy",
            "Cancelled" => "Đã hủy",
            _ => string.IsNullOrWhiteSpace(status) ? "Không xác định" : status
        };
    }
}
