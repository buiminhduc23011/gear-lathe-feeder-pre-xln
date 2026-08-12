using CommunityToolkit.Mvvm.ComponentModel;

namespace Desktop.App.Models.Tray;

/// <summary>
/// Model hiển thị 1 dòng trong bảng order trên AutoPage.
/// </summary>
public partial class OrderDisplayItem : ObservableObject
{
    /// <summary>Thứ tự order trong kệ (1, 2, 3...).</summary>
    [ObservableProperty]
    private int sequence;

    /// <summary>Mã order.</summary>
    [ObservableProperty]
    private string? orderId;

    /// <summary>Tên model sản phẩm.</summary>
    [ObservableProperty]
    private string modelName = "—";

    /// <summary>Số lượng sản phẩm trong order.</summary>
    [ObservableProperty]
    private int quantity;

    /// <summary>Vị trí trên Xe hàng (1-4).</summary>
    [ObservableProperty]
    private int trayIndex;

    [ObservableProperty]
    private int cartPositionIndex;

    /// <summary>Vị trí bắt đầu trên tray (1-indexed).</summary>
    [ObservableProperty]
    private int startPosition;

    /// <summary>Hiển thị Xe hàng.</summary>
    [ObservableProperty]
    private string trayTypeName = "—";

    /// <summary>Tên hiển thị loại tay kẹp, hoặc "—" khi chưa xác định.</summary>
    [ObservableProperty]
    private string jigTypeName = "—";

    /// <summary>"Hoàn thành" / "Đang chạy" / "Chờ".</summary>
    [ObservableProperty]
    private string statusText = "Chờ";

    /// <summary>Badge key: BadgeSuccess / BadgeInfo / BadgeWarning.</summary>
    [ObservableProperty]
    private string statusBadgeKey = "BadgeWarning";
}
