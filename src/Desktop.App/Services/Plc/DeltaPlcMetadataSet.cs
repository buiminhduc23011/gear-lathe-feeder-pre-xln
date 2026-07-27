using System.Collections.ObjectModel;
using Desktop.App.Configuration.Plc;

namespace Desktop.App.Services.Plc;

/// <summary>
/// Instance-based PLC metadata set for a custom tag list.
/// Used by Line1/Line2 PlcService instances that cannot share
/// the static metadata built from PlcTagCatalog.All (Robot).
/// </summary>
public sealed class DeltaPlcMetadataSet
{
    public IReadOnlyList<PlcTagDefinition> Tags { get; }

    public IReadOnlyDictionary<string, DeltaPlcTagBinding> Bindings { get; }

    public IReadOnlyDictionary<DeltaPlcArea, IReadOnlyList<DeltaPlcReadRange>> Ranges { get; }

    public DeltaPlcMetadataSet(IReadOnlyList<PlcTagDefinition> tags)
    {
        Tags = tags;

        Bindings = new ReadOnlyDictionary<string, DeltaPlcTagBinding>(
            tags.ToDictionary(
                static tag => tag.Name,
                static tag => DeltaPlcMetadata.CreateBindingPublic(tag),
                StringComparer.OrdinalIgnoreCase));

        Ranges = new ReadOnlyDictionary<DeltaPlcArea, IReadOnlyList<DeltaPlcReadRange>>(
            Enum.GetValues<DeltaPlcArea>().ToDictionary(
                static area => area,
                area => BuildRangesForArea(area)));
    }

    public DeltaPlcTagBinding GetRequiredBinding(string tagName)
    {
        if (Bindings.TryGetValue(tagName, out var binding))
        {
            return binding;
        }

        throw new KeyNotFoundException($"Unknown PLC tag metadata for '{tagName}'.");
    }

    public IReadOnlyList<DeltaPlcReadRange> GetRanges(DeltaPlcArea area)
    {
        return Ranges[area];
    }

    private IReadOnlyList<DeltaPlcReadRange> BuildRangesForArea(DeltaPlcArea area)
    {
        var segments = Bindings.Values
            .Where(binding => binding.Area == area)
            .Select(static binding => (Start: binding.StartAddress, End: binding.StartAddress + binding.Span - 1))
            .OrderBy(static segment => segment.Start)
            .ToList();

        return DeltaPlcMetadata.BuildRanges(area, segments);
    }
}
