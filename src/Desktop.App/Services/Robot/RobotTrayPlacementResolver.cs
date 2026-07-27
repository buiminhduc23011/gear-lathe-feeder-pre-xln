using Desktop.App.Data.Repositories;
using Desktop.App.Models.Tray;

namespace Desktop.App.Services.Robot;

internal sealed class RobotTrayPlacementResolver
{
    private readonly ITrayConfigRepository _trayConfigRepository;

    public RobotTrayPlacementResolver(ITrayConfigRepository trayConfigRepository)
    {
        _trayConfigRepository = trayConfigRepository;
    }

    public int ResolveTrayType(int shelfLayoutType, int trayIndex)
    {
        var layout = Enum.IsDefined(typeof(ShelfLayoutType), shelfLayoutType)
            ? (ShelfLayoutType)shelfLayoutType
            : ShelfLayoutType.Unknown;

        var (tray1, tray2) = layout.GetTraySizes();
        var traySize = trayIndex switch
        {
            1 => tray1,
            2 => tray2,
            _ => TraySize.Unknown
        };

        return traySize switch
        {
            TraySize.Small => 1,
            TraySize.Large => 2,
            _ => 0
        };
    }

    public async Task<int> ResolveStartPositionAsync(int startPosition, int trayType)
    {
        if (startPosition <= 0)
        {
            return 0;
        }

        var traySize = trayType switch
        {
            1 => TraySize.Small,
            2 => TraySize.Large,
            _ => TraySize.Unknown
        };

        if (traySize == TraySize.Unknown)
        {
            return startPosition;
        }

        var columnCount = await ResolveColumnCountAsync(traySize);
        return (startPosition - 1) * columnCount + 1; // start hàng từ 0, đếm từ 1 nên +1 ở cuối để chuyển về vị trí bắt đầu của hàng tiếp theo
    }

    private async Task<int> ResolveColumnCountAsync(TraySize traySize)
    {
        var defaultColumns = traySize.GetDimensions().Columns;

        try
        {
            var config = await _trayConfigRepository.GetAsync(traySize);
            return config?.Columns > 0 ? config.Columns : defaultColumns;
        }
        catch
        {
            return defaultColumns;
        }
    }
}
