namespace Desktop.App.Configuration.Plc;

public sealed class PlcTagDefinition
{
    private readonly int? _length;

    public required string Name { get; init; }

    public required string Address { get; init; }

    public required PlcTagDataType DataType { get; init; }

    public required string Description { get; init; }

    public int? Length
    {
        get => _length;
        init => _length = value;
    }

    public int? Lenght
    {
        get => _length;
        init => _length = value;
    }
}
