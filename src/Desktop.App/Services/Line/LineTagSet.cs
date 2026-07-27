using Desktop.App.Configuration.Plc;

namespace Desktop.App.Services.Line;

/// <summary>
/// Ánh xạ tag PLC chung cho 1 line (Line 1 hoặc Line 2).
/// Dùng chung bởi <see cref="LineModelPlcService"/>.
/// </summary>
public sealed record LineTagSet
{
    // --- Auto Load Control ---
    public required PlcTagDefinition AutoLoadDataModel { get; init; }
    public required PlcTagDefinition AutoDoneLoadDataModel { get; init; }
    public required PlcTagDefinition AutoModelNameToLoad { get; init; }

    // --- Auto Model Data (D6000-D6049) ---
    public required PlcTagDefinition AutoModelName { get; init; }
    public required PlcTagDefinition AutoJigType { get; init; }
    public required PlcTagDefinition AutoPickInputX { get; init; }
    public required PlcTagDefinition AutoPickInputZ { get; init; }
    public required PlcTagDefinition AutoPickOp1X { get; init; }
    public required PlcTagDefinition AutoPickOp1Z { get; init; }
    public required PlcTagDefinition AutoPickOp2X { get; init; }
    public required PlcTagDefinition AutoPickOp2Z { get; init; }
    public required PlcTagDefinition AutoDropOp1X { get; init; }
    public required PlcTagDefinition AutoDropOp1Z { get; init; }
    public required PlcTagDefinition AutoDropOp2X { get; init; }
    public required PlcTagDefinition AutoDropOp2Z { get; init; }
    public required PlcTagDefinition AutoDropMeasureX { get; init; }
    public required PlcTagDefinition AutoDropMeasureZ { get; init; }
    public required PlcTagDefinition AutoJigSupportInput { get; init; }
    public required PlcTagDefinition AutoGrindTimeOp1 { get; init; }
    public required PlcTagDefinition AutoGrindTimeOp2 { get; init; }
    public required PlcTagDefinition AutoProgramId { get; init; }
    public required PlcTagDefinition AutoDiameterOp1 { get; init; }
    public required PlcTagDefinition AutoDiameterOp2 { get; init; }

    // --- Edit Model Data ---
    public required PlcTagDefinition IdModel { get; init; }
    public required PlcTagDefinition JigType { get; init; }
    public required PlcTagDefinition PickInputX { get; init; }
    public required PlcTagDefinition PickInputZ { get; init; }
    public required PlcTagDefinition PickOp1X { get; init; }
    public required PlcTagDefinition PickOp1Z { get; init; }
    public required PlcTagDefinition PickOp2X { get; init; }
    public required PlcTagDefinition PickOp2Z { get; init; }
    public required PlcTagDefinition DropOp1X { get; init; }
    public required PlcTagDefinition DropOp1Z { get; init; }
    public required PlcTagDefinition DropOp2X { get; init; }
    public required PlcTagDefinition DropOp2Z { get; init; }
    public required PlcTagDefinition DropMeasureX { get; init; }
    public required PlcTagDefinition DropMeasureZ { get; init; }
    public required PlcTagDefinition JigSupportInput { get; init; }
    public required PlcTagDefinition GrindTimeOp1 { get; init; }
    public required PlcTagDefinition GrindTimeOp2 { get; init; }
    public required PlcTagDefinition DiameterOp1 { get; init; }
    public required PlcTagDefinition DiameterOp2 { get; init; }

    // --- Edit Model Search ---
    public required PlcTagDefinition ModelSearchName { get; init; }
    public required PlcTagDefinition ModelResult1 { get; init; }
    public required PlcTagDefinition ModelResult2 { get; init; }
    public required PlcTagDefinition ModelResult3 { get; init; }
    public required PlcTagDefinition ModelResult4 { get; init; }
    public required PlcTagDefinition ModelResult5 { get; init; }
    public required PlcTagDefinition ModelResult6 { get; init; }

    // --- Control Bits ---
    public required PlcTagDefinition Search { get; init; }
    public required PlcTagDefinition Edit { get; init; }
    public required PlcTagDefinition Next { get; init; }
    public required PlcTagDefinition Previous { get; init; }
    public required PlcTagDefinition SaveModel { get; init; }

    // --- Feedback ---
    public required PlcTagDefinition SaveSuccess { get; init; }
    public required PlcTagDefinition ErrorFlag { get; init; }

    // --- Pagination ---
    public required PlcTagDefinition Page { get; init; }
    public required PlcTagDefinition TotalPages { get; init; }

    // --- Error Message ---
    public required PlcTagDefinition ErrorMessage { get; init; }

    // --- Authentication ---
    public required PlcTagDefinition AccountName { get; init; }
    public required PlcTagDefinition Password { get; init; }

    /// <summary>Danh sách 6 tag kết quả tìm kiếm, theo thứ tự.</summary>
    public PlcTagDefinition[] ModelResults =>
    [
        ModelResult1, ModelResult2, ModelResult3,
        ModelResult4, ModelResult5, ModelResult6,
    ];

    public static LineTagSet ForLine1() => new()
    {
        AutoLoadDataModel = PlcTagCatalog.Line1.AutoLoadDataModel,
        AutoDoneLoadDataModel = PlcTagCatalog.Line1.AutoDoneLoadDataModel,
        AutoModelNameToLoad = PlcTagCatalog.Line1.AutoModelNameToLoad,
        AutoModelName = PlcTagCatalog.Line1.AutoModelName,
        AutoJigType = PlcTagCatalog.Line1.AutoJigType,
        AutoPickInputX = PlcTagCatalog.Line1.AutoPickInputX,
        AutoPickInputZ = PlcTagCatalog.Line1.AutoPickInputZ,
        AutoPickOp1X = PlcTagCatalog.Line1.AutoPickOp1X,
        AutoPickOp1Z = PlcTagCatalog.Line1.AutoPickOp1Z,
        AutoPickOp2X = PlcTagCatalog.Line1.AutoPickOp2X,
        AutoPickOp2Z = PlcTagCatalog.Line1.AutoPickOp2Z,
        AutoDropOp1X = PlcTagCatalog.Line1.AutoDropOp1X,
        AutoDropOp1Z = PlcTagCatalog.Line1.AutoDropOp1Z,
        AutoDropOp2X = PlcTagCatalog.Line1.AutoDropOp2X,
        AutoDropOp2Z = PlcTagCatalog.Line1.AutoDropOp2Z,
        AutoDropMeasureX = PlcTagCatalog.Line1.AutoDropMeasureX,
        AutoDropMeasureZ = PlcTagCatalog.Line1.AutoDropMeasureZ,
        AutoJigSupportInput = PlcTagCatalog.Line1.AutoJigSupportInput,
        AutoGrindTimeOp1 = PlcTagCatalog.Line1.AutoGrindTimeOp1,
        AutoGrindTimeOp2 = PlcTagCatalog.Line1.AutoGrindTimeOp2,
        AutoProgramId = PlcTagCatalog.Line1.AutoProgramId,
        AutoDiameterOp1 = PlcTagCatalog.Line1.AutoDiameterOp1,
        AutoDiameterOp2 = PlcTagCatalog.Line1.AutoDiameterOp2,
        IdModel = PlcTagCatalog.Line1.IdModel,
        JigType = PlcTagCatalog.Line1.JigType,
        PickInputX = PlcTagCatalog.Line1.PickInputX,
        PickInputZ = PlcTagCatalog.Line1.PickInputZ,
        PickOp1X = PlcTagCatalog.Line1.PickOp1X,
        PickOp1Z = PlcTagCatalog.Line1.PickOp1Z,
        PickOp2X = PlcTagCatalog.Line1.PickOp2X,
        PickOp2Z = PlcTagCatalog.Line1.PickOp2Z,
        DropOp1X = PlcTagCatalog.Line1.DropOp1X,
        DropOp1Z = PlcTagCatalog.Line1.DropOp1Z,
        DropOp2X = PlcTagCatalog.Line1.DropOp2X,
        DropOp2Z = PlcTagCatalog.Line1.DropOp2Z,
        DropMeasureX = PlcTagCatalog.Line1.DropMeasureX,
        DropMeasureZ = PlcTagCatalog.Line1.DropMeasureZ,
        JigSupportInput = PlcTagCatalog.Line1.JigSupportInput,
        GrindTimeOp1 = PlcTagCatalog.Line1.GrindTimeOp1,
        GrindTimeOp2 = PlcTagCatalog.Line1.GrindTimeOp2,
        DiameterOp1 = PlcTagCatalog.Line1.DiameterOp1,
        DiameterOp2 = PlcTagCatalog.Line1.DiameterOp2,
        ModelSearchName = PlcTagCatalog.Line1.ModelSearchName,
        ModelResult1 = PlcTagCatalog.Line1.ModelResult1,
        ModelResult2 = PlcTagCatalog.Line1.ModelResult2,
        ModelResult3 = PlcTagCatalog.Line1.ModelResult3,
        ModelResult4 = PlcTagCatalog.Line1.ModelResult4,
        ModelResult5 = PlcTagCatalog.Line1.ModelResult5,
        ModelResult6 = PlcTagCatalog.Line1.ModelResult6,
        Search = PlcTagCatalog.Line1.Search,
        Edit = PlcTagCatalog.Line1.EditModel,
        Next = PlcTagCatalog.Line1.Next,
        Previous = PlcTagCatalog.Line1.Previous,
        SaveModel = PlcTagCatalog.Line1.SaveModel,
        SaveSuccess = PlcTagCatalog.Line1.SaveSuccess,
        ErrorFlag = PlcTagCatalog.Line1.ErrorFlag,
        Page = PlcTagCatalog.Line1.Page,
        TotalPages = PlcTagCatalog.Line1.TotalPages,
        ErrorMessage = PlcTagCatalog.Line1.ErrorMessage,
        AccountName = PlcTagCatalog.Line1.AccountName,
        Password = PlcTagCatalog.Line1.Password,
    };

    public static LineTagSet ForLine2() => new()
    {
        AutoLoadDataModel = PlcTagCatalog.Line2.AutoLoadDataModel,
        AutoDoneLoadDataModel = PlcTagCatalog.Line2.AutoDoneLoadDataModel,
        AutoModelNameToLoad = PlcTagCatalog.Line2.AutoModelNameToLoad,
        AutoModelName = PlcTagCatalog.Line2.AutoModelName,
        AutoJigType = PlcTagCatalog.Line2.AutoJigType,
        AutoPickInputX = PlcTagCatalog.Line2.AutoPickInputX,
        AutoPickInputZ = PlcTagCatalog.Line2.AutoPickInputZ,
        AutoPickOp1X = PlcTagCatalog.Line2.AutoPickOp1X,
        AutoPickOp1Z = PlcTagCatalog.Line2.AutoPickOp1Z,
        AutoPickOp2X = PlcTagCatalog.Line2.AutoPickOp2X,
        AutoPickOp2Z = PlcTagCatalog.Line2.AutoPickOp2Z,
        AutoDropOp1X = PlcTagCatalog.Line2.AutoDropOp1X,
        AutoDropOp1Z = PlcTagCatalog.Line2.AutoDropOp1Z,
        AutoDropOp2X = PlcTagCatalog.Line2.AutoDropOp2X,
        AutoDropOp2Z = PlcTagCatalog.Line2.AutoDropOp2Z,
        AutoDropMeasureX = PlcTagCatalog.Line2.AutoDropMeasureX,
        AutoDropMeasureZ = PlcTagCatalog.Line2.AutoDropMeasureZ,
        AutoJigSupportInput = PlcTagCatalog.Line2.AutoJigSupportInput,
        AutoGrindTimeOp1 = PlcTagCatalog.Line2.AutoGrindTimeOp1,
        AutoGrindTimeOp2 = PlcTagCatalog.Line2.AutoGrindTimeOp2,
        AutoProgramId = PlcTagCatalog.Line2.AutoProgramId,
        AutoDiameterOp1 = PlcTagCatalog.Line2.AutoDiameterOp1,
        AutoDiameterOp2 = PlcTagCatalog.Line2.AutoDiameterOp2,
        IdModel = PlcTagCatalog.Line2.IdModel,
        JigType = PlcTagCatalog.Line2.JigType,
        PickInputX = PlcTagCatalog.Line2.PickInputX,
        PickInputZ = PlcTagCatalog.Line2.PickInputZ,
        PickOp1X = PlcTagCatalog.Line2.PickOp1X,
        PickOp1Z = PlcTagCatalog.Line2.PickOp1Z,
        PickOp2X = PlcTagCatalog.Line2.PickOp2X,
        PickOp2Z = PlcTagCatalog.Line2.PickOp2Z,
        DropOp1X = PlcTagCatalog.Line2.DropOp1X,
        DropOp1Z = PlcTagCatalog.Line2.DropOp1Z,
        DropOp2X = PlcTagCatalog.Line2.DropOp2X,
        DropOp2Z = PlcTagCatalog.Line2.DropOp2Z,
        DropMeasureX = PlcTagCatalog.Line2.DropMeasureX,
        DropMeasureZ = PlcTagCatalog.Line2.DropMeasureZ,
        JigSupportInput = PlcTagCatalog.Line2.JigSupportInput,
        GrindTimeOp1 = PlcTagCatalog.Line2.GrindTimeOp1,
        GrindTimeOp2 = PlcTagCatalog.Line2.GrindTimeOp2,
        DiameterOp1 = PlcTagCatalog.Line2.DiameterOp1,
        DiameterOp2 = PlcTagCatalog.Line2.DiameterOp2,
        ModelSearchName = PlcTagCatalog.Line2.ModelSearchName,
        ModelResult1 = PlcTagCatalog.Line2.ModelResult1,
        ModelResult2 = PlcTagCatalog.Line2.ModelResult2,
        ModelResult3 = PlcTagCatalog.Line2.ModelResult3,
        ModelResult4 = PlcTagCatalog.Line2.ModelResult4,
        ModelResult5 = PlcTagCatalog.Line2.ModelResult5,
        ModelResult6 = PlcTagCatalog.Line2.ModelResult6,
        Search = PlcTagCatalog.Line2.Search,
        Edit = PlcTagCatalog.Line2.EditModel,
        Next = PlcTagCatalog.Line2.Next,
        Previous = PlcTagCatalog.Line2.Previous,
        SaveModel = PlcTagCatalog.Line2.SaveModel,
        SaveSuccess = PlcTagCatalog.Line2.SaveSuccess,
        ErrorFlag = PlcTagCatalog.Line2.ErrorFlag,
        Page = PlcTagCatalog.Line2.Page,
        TotalPages = PlcTagCatalog.Line2.TotalPages,
        ErrorMessage = PlcTagCatalog.Line2.ErrorMessage,
        AccountName = PlcTagCatalog.Line2.AccountName,
        Password = PlcTagCatalog.Line2.Password,
    };
}
