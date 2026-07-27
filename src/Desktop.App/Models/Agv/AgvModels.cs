namespace Desktop.App.Models.Agv;

public enum AgvPosition
{
    Position1 = 1,
    Position2 = 2
}

public enum AgvCallStatus
{
    Pending = 0,
    Calling = 1,
    InProgress = 2,
    Completed = 3,
    Failed = 4,
    Cancelled = 5,
    AwaitingCompletion = 6
}

public enum AgvTransferStatus
{
    None = 0,
    HasCommand = 1,
    Ready = 2,
    Completed = 3
}

public class AgvCallRecord
{
    public int Id { get; set; }
    public AgvPosition Position { get; set; }
    public AgvCallStatus Status { get; set; }
    public bool IsAutoCall { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public string? Note { get; set; }
    public int RemainingQty { get; set; }

    public string PositionDisplay => Position == AgvPosition.Position1 ? "Kệ 1" : "Kệ 2";

    public string StatusDisplay => Status switch
    {
        AgvCallStatus.Pending => "Chờ gọi",
        AgvCallStatus.Calling => "Đang gọi",
        AgvCallStatus.InProgress => "Đang chạy",
        AgvCallStatus.AwaitingCompletion => "Chờ hoàn tất",
        AgvCallStatus.Completed => "Hoàn thành",
        AgvCallStatus.Failed => "Lỗi",
        AgvCallStatus.Cancelled => "Đã hủy",
        _ => Status.ToString()
    };

    public string MethodDisplay => IsAutoCall ? "Tự động" : "Thủ công";

    public string DurationText
    {
        get
        {
            if (CompletedAtUtc.HasValue)
            {
                var diff = CompletedAtUtc.Value - CreatedAtUtc;
                return $"{(int)diff.TotalMinutes}m {diff.Seconds}s";
            }

            if (StartedAtUtc.HasValue)
            {
                var diff = DateTime.UtcNow - StartedAtUtc.Value;
                return $"{(int)diff.TotalMinutes}m {diff.Seconds}s (...)";
            }

            return "-";
        }
    }
}

public enum ManualLoadResultStatus
{
    Success = 0,
    NoPendingDeclaration = 1,
    PositionNotClear = 2,
    Failed = 3,
    BlockedByInactiveModel = 4,
}

public sealed record ManualLoadResult
{
    public ManualLoadResultStatus Status { get; init; }
    public string Message { get; init; } = string.Empty;
    public int? DeclarationId { get; init; }

    public bool IsSuccess => Status == ManualLoadResultStatus.Success;
}
