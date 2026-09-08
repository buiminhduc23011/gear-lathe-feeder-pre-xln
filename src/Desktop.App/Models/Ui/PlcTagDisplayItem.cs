using CommunityToolkit.Mvvm.ComponentModel;

namespace Desktop.App.Models.Ui;

public partial class PlcTagDisplayItem : ObservableObject
{
    public required int Index { get; init; }

    public required string Name { get; init; }

    public required string Address { get; init; }

    public required string DataType { get; init; }

    public required string Description { get; init; }

    [ObservableProperty]
    private string value = string.Empty;

    [ObservableProperty]
    private string status = "NG";

    [ObservableProperty]
    private DateTime? lastUpdatedTime;
}
