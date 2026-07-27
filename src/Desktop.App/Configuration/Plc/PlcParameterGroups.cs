namespace Desktop.App.Configuration.Plc;

public static class PlcParameterGroups
{
    public const string DataTrayCart = "DataTrayCart";
    public const string DataMachine = "DataMachine";
    public const string DataOriginCheck = "DataOriginCheck";

    public static IReadOnlyList<string> All { get; } =
    [
        DataTrayCart,
        DataMachine,
        DataOriginCheck,
    ];

    public static IReadOnlyList<PlcTagDefinition> GetTags(string groupName)
    {
        return groupName switch
        {
            DataTrayCart => DataTrayCartTags,
            DataMachine => DataMachineTags,
            DataOriginCheck => DataOriginCheckTags,
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

        if (DataOriginCheckTagNames.Contains(tagName))
        {
            groupName = DataOriginCheck;
            return true;
        }

        groupName = string.Empty;
        return false;
    }

    private static IReadOnlySet<string> DataTrayCartTagNames { get; } = CollectTagNames(typeof(PlcTagCatalog.DataTrayCart));

    private static IReadOnlySet<string> DataMachineTagNames { get; } = CollectTagNames(typeof(PlcTagCatalog.DataMachine));

    private static IReadOnlySet<string> DataOriginCheckTagNames { get; } = CollectTagNames(typeof(PlcTagCatalog.DataOriginCheck));

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

    private static IReadOnlyList<PlcTagDefinition> DataOriginCheckTags { get; } =
        PlcTagCatalog.All
            .Where(static tag => tag is not null)
            .Where(tag => DataOriginCheckTagNames.Contains(tag!.Name))
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
