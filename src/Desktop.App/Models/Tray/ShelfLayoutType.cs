namespace Desktop.App.Models.Tray;

/// <summary>
/// Loại bố trí tray trong kệ, do AGV gửi xuống.
/// Quy ước local của Desktop: Tray 1 = dưới, Tray 2 = trên.
/// </summary>
public enum ShelfLayoutType
{
    /// <summary>Chưa xác định.</summary>
    Unknown = 0,

    /// <summary>2 tray nhỏ (trên nhỏ, dưới nhỏ).</summary>
    TwoSmall = 1,

    /// <summary>2 tray lớn (trên lớn, dưới lớn).</summary>
    TwoLarge = 2,

    /// <summary>Dưới nhỏ, trên lớn.</summary>
    SmallBottomLargeTop = 3,

    /// <summary>Dưới lớn, trên nhỏ.</summary>
    LargeBottomSmallTop = 4
}

public static class ShelfLayoutTypeExtensions
{
    /// <summary>
    /// Trả về (Tray1Size, Tray2Size) dựa vào layout type.
    /// Tray 1 = dưới, Tray 2 = trên.
    /// </summary>
    public static (TraySize Tray1, TraySize Tray2) GetTraySizes(this ShelfLayoutType layout)
    {
        return layout switch
        {
            ShelfLayoutType.TwoSmall => (TraySize.Small, TraySize.Small),
            ShelfLayoutType.TwoLarge => (TraySize.Large, TraySize.Large),
            ShelfLayoutType.SmallBottomLargeTop => (TraySize.Small, TraySize.Large),
            ShelfLayoutType.LargeBottomSmallTop => (TraySize.Large, TraySize.Small),
            _ => (TraySize.Small, TraySize.Small)
        };
    }

    public static string GetDisplayText(this ShelfLayoutType layout) => layout switch
    {
        ShelfLayoutType.TwoSmall => "2 Tray Nhỏ",
        ShelfLayoutType.TwoLarge => "2 Tray Lớn",
        ShelfLayoutType.SmallBottomLargeTop => "Tray nhỏ dưới + Tray lớn trên",
        ShelfLayoutType.LargeBottomSmallTop => "Tray lớn dưới + Tray nhỏ trên",
        _ => "Chưa xác định"
    };

    public static (int Rows, int Columns) GetDimensions(this TraySize size)
    {
        return size switch
        {
            TraySize.Large => (4, 8),
            _ => (5, 9)
        };
    }
}
