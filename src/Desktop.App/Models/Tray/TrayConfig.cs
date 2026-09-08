namespace Desktop.App.Models.Tray;

public enum TraySize
{
    Unknown = 0,
    Small = 1,
    Large = 2
}

/// <summary>
/// Cấu hình cho 1 loại tray (Bé hoặc Lớn).
/// Lưu trữ trong SQLite bảng tray_configs.
/// Settings chỉ cài đặt 2 bản ghi: Small + Large.
/// </summary>
public class TrayConfig
{
    public int Id { get; set; }

    /// <summary>Loại tray: Small hoặc Large.</summary>
    public TraySize Size { get; set; } = TraySize.Small;

    /// <summary>Số hàng (mặc định 5 cho bé, 4 cho lớn).</summary>
    public int Rows { get; set; } = 5;

    /// <summary>Số cột (mặc định 9 cho bé, 8 cho lớn).</summary>
    public int Columns { get; set; } = 9;

    /// <summary>Khoảng cách giữa 2 hàng (mm). Dùng để vẽ đúng tỷ lệ và gửi PLC sau.</summary>
    public float RowOffset { get; set; }

    /// <summary>Khoảng cách giữa 2 cột (mm). Dùng để vẽ đúng tỷ lệ và gửi PLC sau.</summary>
    public float ColOffset { get; set; }

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>Tổng số vị trí trên tray.</summary>
    public int TotalSlots => Rows * Columns;

    /// <summary>Tạo default config cho tray bé (5×9).</summary>
    public static TrayConfig CreateSmallDefault()
    {
        return new TrayConfig
        {
            Size = TraySize.Small,
            Rows = 5,
            Columns = 9,
            RowOffset = 55.0f,
            ColOffset = 55.0f,
            UpdatedAtUtc = DateTime.UtcNow
        };
    }

    /// <summary>Tạo default config cho tray lớn (4×8).</summary>
    public static TrayConfig CreateLargeDefault()
    {
        return new TrayConfig
        {
            Size = TraySize.Large,
            Rows = 4,
            Columns = 8,
            RowOffset = 68.0f,
            ColOffset = 68.0f,
            UpdatedAtUtc = DateTime.UtcNow
        };
    }
}
