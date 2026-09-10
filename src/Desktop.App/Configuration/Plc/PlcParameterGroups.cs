namespace Desktop.App.Configuration.Plc;

public static class PlcParameterGroups
{
    public const string DataTrayCart = "DataTrayCart";
    public const string DataMachine = "DataMachine";
    public const string DataSafeCoordinates = "DataSafeCoordinates";
    public const string DataInputGroup = "DataInputGroup";

    public static IReadOnlyList<string> All { get; } =
    [
        DataTrayCart,
        DataMachine,
        DataSafeCoordinates,
        DataInputGroup,
    ];

    public static IReadOnlyList<PlcTagDefinition> GetTags(string groupName)
    {
        return groupName switch
        {
            DataTrayCart => DataTrayCartTags,
            DataMachine => DataMachineTags,
            DataSafeCoordinates => DataSafeCoordinatesTags,
            DataInputGroup => DataInputGroupTags,
            _ => throw new ArgumentOutOfRangeException(nameof(groupName), groupName, "Unknown PLC parameter group."),
        };
    }

    public static bool TryGetGroupName(string tagName, out string groupName)
    {
        if (DataTrayCartTagNames.Contains(tagName))
        {
            groupName = DataTrayCart;
            return true;
        }

        if (DataMachineTagNames.Contains(tagName))
        {
            groupName = DataMachine;
            return true;
        }

        if (DataSafeCoordinatesTagNames.Contains(tagName))
        {
            groupName = DataSafeCoordinates;
            return true;
        }

        if (DataInputGroupTagNames.Contains(tagName))
        {
            groupName = DataInputGroup;
            return true;
        }

        groupName = string.Empty;
        return false;
    }

    private static IReadOnlySet<string> DataTrayCartTagNames { get; } = CollectTagNames(typeof(PlcTagCatalog.DataTrayCart));

    private static IReadOnlySet<string> DataMachineTagNames { get; } = CollectTagNames(typeof(PlcTagCatalog.DataMachine));

    private static IReadOnlySet<string> DataSafeCoordinatesTagNames { get; } = CollectTagNames(typeof(PlcTagCatalog.DataSafeCoordinates));

    private static IReadOnlySet<string> DataInputGroupTagNames { get; } = CollectTagNames(typeof(PlcTagCatalog.DataInputGroup));

    private static IReadOnlyList<PlcTagDefinition> DataTrayCartTags { get; } =
        PlcTagCatalog.All
            .Where(static tag => tag is not null)
            .Where(tag => DataTrayCartTagNames.Contains(tag!.Name))
            .ToArray();

    private static IReadOnlyList<PlcTagDefinition> DataMachineTags { get; } =
        PlcTagCatalog.All
            .Where(static tag => tag is not null)
            .Where(tag => DataMachineTagNames.Contains(tag!.Name))
            .ToArray();

    private static IReadOnlyList<PlcTagDefinition> DataSafeCoordinatesTags { get; } =
        PlcTagCatalog.All
            .Where(static tag => tag is not null)
            .Where(tag => DataSafeCoordinatesTagNames.Contains(tag!.Name))
            .ToArray();

    private static IReadOnlyList<PlcTagDefinition> DataInputGroupTags { get; } =
        PlcTagCatalog.All
            .Where(static tag => tag is not null)
            .Where(tag => DataInputGroupTagNames.Contains(tag!.Name))
            .ToArray();

    private static IReadOnlySet<string> CollectTagNames(Type groupType)
    {
        return new HashSet<string>(
            groupType
                .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.DeclaredOnly)
                .Where(static field => field.FieldType == typeof(PlcTagDefinition))
                .Select(static field => ((PlcTagDefinition?)field.GetValue(null))?.Name)
                .OfType<string>(),
            StringComparer.OrdinalIgnoreCase);
    }
}
