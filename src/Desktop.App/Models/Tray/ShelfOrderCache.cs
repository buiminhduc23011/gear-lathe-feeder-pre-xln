namespace Desktop.App.Models.Tray;

/// <summary>
/// Cache trạng thái đang chạy của 1 machine slot.
/// Lưu trữ trong SQLite bảng shelf_order_cache để khôi phục sau khi app khởi động lại.
/// </summary>
public class ShelfOrderCache
{
    public int Id { get; set; }

    /// <summary>Machine slot 1 hoặc 2.</summary>
    public int KeIndex { get; set; }

    public int? DeclarationId { get; set; }

    public int CurrentOrderSequence { get; set; }

    public int TotalOrders { get; set; }

    public int ShelfLayoutType { get; set; }

    public int CurrentItemInOrder { get; set; }

    public bool AwaitingPickReset { get; set; }

    public bool CompletionReported { get; set; }

    public bool CompletionAcknowledged { get; set; }

    public int OrderQtySnapshot { get; set; }

    public int RanQtySnapshot { get; set; }

    public string OrdersJson { get; set; } = "[]";

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
