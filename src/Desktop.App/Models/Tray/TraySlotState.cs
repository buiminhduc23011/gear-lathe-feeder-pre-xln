using CommunityToolkit.Mvvm.ComponentModel;

namespace Desktop.App.Models.Tray;

/// <summary>Trạng thái hiển thị 1 ô trên tray.</summary>
public enum SlotStatus
{
    /// <summary>Trống (không có order hoặc đã gắp xong).</summary>
    Empty,

    /// <summary>Có sản phẩm (chờ gắp).</summary>
    HasProduct,

    /// <summary>Đang gắp (con hàng hiện tại).</summary>
    Picking
}

/// <summary>
/// Trạng thái 1 ô trên tray grid, dùng cho data-binding trên UI.
/// Mỗi ô hiển thị dưới dạng hình tròn với màu sắc theo Status.
/// </summary>
public partial class TraySlotState : ObservableObject
{
    /// <summary>Vị trí 1-indexed trên tray (trái→phải, trên→dưới).</summary>
    [ObservableProperty]
    private int position;

    /// <summary>Hàng (0-indexed).</summary>
    [ObservableProperty]
    private int row;

    /// <summary>Cột (0-indexed).</summary>
    [ObservableProperty]
    private int column;

    /// <summary>Trạng thái hiển thị.</summary>
    [ObservableProperty]
    private SlotStatus status = SlotStatus.Empty;

    /// <summary>Order nào sở hữu ô này (null = không có order).</summary>
    [ObservableProperty]
    private string? orderId;

    /// <summary>Tên model sản phẩm tại ô này (null = không có order).</summary>
    [ObservableProperty]
    private string? modelName;
}
