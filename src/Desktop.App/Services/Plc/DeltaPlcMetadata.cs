using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Desktop.App.Configuration.Plc;

namespace Desktop.App.Services.Plc;

public enum DeltaPlcArea
{
    DWord,
    M,
    X,
    Y,
}

public sealed record DeltaPlcTagBinding(
    PlcTagDefinition Tag,
    DeltaPlcArea Area,
    int StartAddress,
    int Span,
    int? BitIndex = null);

public sealed record DeltaPlcReadRange(DeltaPlcArea Area, int StartAddress, int Count);

internal static partial class DeltaPlcMetadata
{
    internal const int MaxRegistersPerRead = 100;

    // Maximum number of gap registers allowed to be filled when merging consecutive groups.
    // Keeps merged reads dense; prevents crossing large address boundaries that may confuse
    // the Delta PLC driver or produce compressed (non-positional) responses.
    internal const int MaxGapFill = 10;
    private static readonly IReadOnlyDictionary<string, DeltaPlcTagBinding> BindingLookup =
        new ReadOnlyDictionary<string, DeltaPlcTagBinding>(
            PlcTagCatalog.All.ToDictionary(
                static tag => tag.Name,
                static tag => CreateBinding(tag),
                StringComparer.OrdinalIgnoreCase));

    private static readonly IReadOnlyDictionary<DeltaPlcArea, IReadOnlyList<DeltaPlcReadRange>> RangeLookup =
        new ReadOnlyDictionary<DeltaPlcArea, IReadOnlyList<DeltaPlcReadRange>>(
            Enum.GetValues<DeltaPlcArea>().ToDictionary(
                static area => area,
                static area => (IReadOnlyList<DeltaPlcReadRange>)BuildRanges(area)));

    public static IReadOnlyDictionary<string, DeltaPlcTagBinding> Bindings => BindingLookup;

    public static DeltaPlcTagBinding GetRequiredBinding(string tagName)
    {
        if (Bindings.TryGetValue(tagName, out var binding))
        {
            return binding;
        }

        throw new KeyNotFoundException($"Unknown PLC tag metadata for '{tagName}'.");
    }

    public static IReadOnlyList<DeltaPlcReadRange> GetRanges(DeltaPlcArea area)
    {
        return RangeLookup[area];
    }

    /// <summary>
    /// Build an instance-based metadata set for a custom tag list (e.g. Line1/Line2 tags).
    /// </summary>
    public static DeltaPlcMetadataSet BuildFor(IReadOnlyList<PlcTagDefinition> tags)
    {
        return new DeltaPlcMetadataSet(tags);
    }

    private static IReadOnlyList<DeltaPlcReadRange> BuildRanges(DeltaPlcArea area)
    {
        var segments = Bindings.Values
            .Where(binding => binding.Area == area)
            .Select(static binding => (Start: binding.StartAddress, End: binding.StartAddress + binding.Span - 1))
            .OrderBy(static segment => segment.Start)
            .ToList();

        return BuildRanges(area, segments);
    }

    internal static IReadOnlyList<DeltaPlcReadRange> BuildRanges(
        DeltaPlcArea area,
        IReadOnlyList<(int Start, int End)> segments,
        int maxRegistersPerRead = MaxRegistersPerRead,
        int maxGapFill = MaxGapFill)
    {
        if (segments.Count == 0)
        {
            return [];
        }

        // Merge overlapping/adjacent segments into contiguous groups.
        var sorted = segments.OrderBy(static s => s.Start).ToList();
        var groups = new List<(int Start, int End)>();
        var currentStart = sorted[0].Start;
        var currentEnd = sorted[0].End;

        for (var index = 1; index < sorted.Count; index++)
        {
            var segment = sorted[index];
            if (segment.Start <= currentEnd + 1)
            {
                currentEnd = Math.Max(currentEnd, segment.End);
                continue;
            }

            groups.Add((currentStart, currentEnd));
            currentStart = segment.Start;
            currentEnd = segment.End;
        }

        groups.Add((currentStart, currentEnd));

        // Gap-filling merge — combine consecutive groups when the gap is small
        //          enough (≤ maxGapFill) AND the merged range fits within maxRegistersPerRead.
        //          This prevents crossing large PLC address boundaries that may cause the
        //          Delta driver to return incorrect or compressed results.
        var merged = new List<(int Start, int End)> { groups[0] };

        for (var index = 1; index < groups.Count; index++)
        {
            var last = merged[^1];
            var next = groups[index];
            var gap = next.Start - last.End - 1;
            var combinedCount = next.End - last.Start + 1;

            if (gap <= maxGapFill && combinedCount <= maxRegistersPerRead)
            {
                merged[^1] = (last.Start, next.End);
            }
            else
            {
                merged.Add(next);
            }
        }

        // Split any range exceeding maxRegistersPerRead into chunks.
        var ranges = new List<DeltaPlcReadRange>();

        foreach (var (start, end) in merged)
        {
            var count = end - start + 1;
            var offset = 0;

            while (offset < count)
            {
                var chunkSize = Math.Min(maxRegistersPerRead, count - offset);
                ranges.Add(new DeltaPlcReadRange(area, start + offset, chunkSize));
                offset += chunkSize;
            }
        }

        return ranges;
    }

    internal static DeltaPlcTagBinding CreateBindingPublic(PlcTagDefinition tag) => CreateBinding(tag);

    private static DeltaPlcTagBinding CreateBinding(PlcTagDefinition tag)
    {
        var address = tag.Address.Trim().ToUpperInvariant();

        if (tag.DataType == PlcTagDataType.Bool && address.StartsWith('M'))
        {
            return new DeltaPlcTagBinding(tag, DeltaPlcArea.M, ParseNumericSuffix(address[1..], address), 1);
        }

        if (tag.DataType == PlcTagDataType.Bool && address.StartsWith('X'))
        {
            return new DeltaPlcTagBinding(tag, DeltaPlcArea.X, ParseDiscreteBitAddress(address, 'X'), 1);
        }

        if (tag.DataType == PlcTagDataType.Bool && address.StartsWith('Y'))
        {
            return new DeltaPlcTagBinding(tag, DeltaPlcArea.Y, ParseDiscreteBitAddress(address, 'Y'), 1);
        }

        if (tag.DataType == PlcTagDataType.Int32 && address.StartsWith("DI", StringComparison.Ordinal))
        {
            return new DeltaPlcTagBinding(tag, DeltaPlcArea.DWord, ParseNumericSuffix(address[2..], address), 2);
        }

        if (tag.DataType == PlcTagDataType.Float && address.StartsWith('R'))
        {
            return new DeltaPlcTagBinding(tag, DeltaPlcArea.DWord, ParseNumericSuffix(address[1..], address), 2);
        }

        if (address.StartsWith('D'))
        {
            return CreateDWordBinding(tag, address);
        }

        throw new NotSupportedException($"PLC address '{tag.Address}' is not supported for tag '{tag.Name}'.");
    }

    private static DeltaPlcTagBinding CreateDWordBinding(PlcTagDefinition tag, string address)
    {
        if (tag.DataType == PlcTagDataType.Bool && address.Contains('.'))
        {
            var parts = address[1..].Split('.', 2);
            var startAddress = ParseNumericSuffix(parts[0], address);
            var bitIndex = ParseNumericSuffix(parts[1], address);
            if (bitIndex is < 0 or > 15)
            {
                throw new NotSupportedException($"PLC bit address '{tag.Address}' is outside the supported D-word bit range.");
            }

            return new DeltaPlcTagBinding(tag, DeltaPlcArea.DWord, startAddress, 1, bitIndex);
        }

        return tag.DataType switch
        {
            PlcTagDataType.Int16 => new DeltaPlcTagBinding(tag, DeltaPlcArea.DWord, ParseNumericSuffix(address[1..], address), 1),
            PlcTagDataType.Int32 => new DeltaPlcTagBinding(tag, DeltaPlcArea.DWord, ParseNumericSuffix(address[1..], address), 2),
            PlcTagDataType.Float => new DeltaPlcTagBinding(tag, DeltaPlcArea.DWord, ParseNumericSuffix(address[1..], address), 2),
            PlcTagDataType.String => new DeltaPlcTagBinding(
                tag,
                DeltaPlcArea.DWord,
                ParseNumericSuffix(address[1..], address),
                CalculateStringWordSpan(tag)),
            _ => throw new NotSupportedException($"PLC D-word address '{tag.Address}' is not supported for tag '{tag.Name}'."),
        };
    }

    private static int CalculateStringWordSpan(PlcTagDefinition tag)
    {
        var length = tag.Length ?? tag.Lenght
            ?? throw new InvalidOperationException($"String tag '{tag.Name}' is missing a fixed length.");

        return (length + 1) / 2;
    }

    private static int ParseDiscreteBitAddress(string address, char prefix)
    {
        var match = DiscreteBitAddressRegex().Match(address);
        if (!match.Success || !string.Equals(match.Groups["prefix"].Value, prefix.ToString(), StringComparison.Ordinal))
        {
            throw new NotSupportedException($"PLC discrete address '{address}' is invalid.");
        }

        var block = ParseNumericSuffix(match.Groups["block"].Value, address);
        if (!int.TryParse(match.Groups["bit"].Value, out var bitIndex) || bitIndex is < 0 or > 15)
        {
            throw new NotSupportedException($"PLC discrete address '{address}' has an invalid bit index.");
        }

        return (block * 16) + bitIndex;
    }

    private static int ParseNumericSuffix(string value, string fullAddress)
    {
        if (int.TryParse(value, out var parsedValue))
        {
            return parsedValue;
        }

        throw new NotSupportedException($"PLC address '{fullAddress}' has an invalid numeric value.");
    }

    [GeneratedRegex("^(?<prefix>[XY])(?<block>\\d+)\\.(?<bit>\\d+)$", RegexOptions.CultureInvariant)]
    private static partial Regex DiscreteBitAddressRegex();
}
