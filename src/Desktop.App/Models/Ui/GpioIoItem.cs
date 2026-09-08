using CommunityToolkit.Mvvm.ComponentModel;

namespace Desktop.App.Models.Ui;

public partial class GpioIoItem : ObservableObject
{
    public required string TagName { get; init; }

    public required string Address { get; init; }

    public required string Name { get; init; }

    [ObservableProperty]
    private bool state;
}
