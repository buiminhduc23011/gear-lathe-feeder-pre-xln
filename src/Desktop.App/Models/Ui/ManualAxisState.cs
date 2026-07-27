using CommunityToolkit.Mvvm.ComponentModel;
using Desktop.App.Configuration.Plc;

namespace Desktop.App.Models.Ui;

public partial class ManualAxisState : ObservableObject
{
    private bool _isApplyingPlcInputs;

    public ManualAxisState(
        string key,
        string displayName,
        string negativeLabel,
        string positiveLabel,
        PlcTagDefinition negativeJogTag,
        PlcTagDefinition positiveJogTag,
        PlcTagDefinition homeTag,
        PlcTagDefinition moveToPointTag,
        PlcTagDefinition manualSpeedTag,
        PlcTagDefinition movePointTag,
        PlcTagDefinition currentPositionTag,
        PlcTagDefinition isHomingTag,
        PlcTagDefinition isHomedTag,
        PlcTagDefinition negativeLimitAlarmTag,
        PlcTagDefinition positiveLimitAlarmTag,
        PlcTagDefinition servoTag)
    {
        Key = key;
        DisplayName = displayName;
        NegativeLabel = negativeLabel;
        PositiveLabel = positiveLabel;
        NegativeJogTag = negativeJogTag;
        PositiveJogTag = positiveJogTag;
        HomeTag = homeTag;
        MoveToPointTag = moveToPointTag;
        ManualSpeedTag = manualSpeedTag;
        MovePointTag = movePointTag;
        CurrentPositionTag = currentPositionTag;
        IsHomingTag = isHomingTag;
        IsHomedTag = isHomedTag;
        NegativeLimitAlarmTag = negativeLimitAlarmTag;
        PositiveLimitAlarmTag = positiveLimitAlarmTag;
        ServoTag = servoTag;

        ManualSpeedInput = "0";
        MovePointInput = "0";
    }

    public string Key { get; }

    public string DisplayName { get; }

    public string NegativeLabel { get; }

    public string PositiveLabel { get; }

    public string NegativeJogTagName => NegativeJogTag.Name;

    public string PositiveJogTagName => PositiveJogTag.Name;

    public string HomeTagName => HomeTag.Name;

    public string MoveToPointTagName => MoveToPointTag.Name;

    internal PlcTagDefinition NegativeJogTag { get; }

    internal PlcTagDefinition PositiveJogTag { get; }

    internal PlcTagDefinition HomeTag { get; }

    internal PlcTagDefinition MoveToPointTag { get; }

    internal PlcTagDefinition ManualSpeedTag { get; }

    internal PlcTagDefinition MovePointTag { get; }

    internal PlcTagDefinition CurrentPositionTag { get; }

    internal PlcTagDefinition IsHomingTag { get; }

    internal PlcTagDefinition IsHomedTag { get; }

    internal PlcTagDefinition NegativeLimitAlarmTag { get; }

    internal PlcTagDefinition PositiveLimitAlarmTag { get; }

    internal PlcTagDefinition ServoTag { get; }

    [ObservableProperty]
    private float currentPosition;

    [ObservableProperty]
    private bool isNegativeJogActive;

    [ObservableProperty]
    private bool isPositiveJogActive;

    [ObservableProperty]
    private bool isHomeCommandActive;

    [ObservableProperty]
    private bool isMoveToPointCommandActive;

    [ObservableProperty]
    private bool isHoming;

    [ObservableProperty]
    private bool isHomed;

    [ObservableProperty]
    private bool isNegativeLimitActive;

    [ObservableProperty]
    private bool isPositiveLimitActive;

    [ObservableProperty]
    private bool isServoOn;

    [ObservableProperty]
    private string manualSpeedInput = string.Empty;

    [ObservableProperty]
    private string movePointInput = string.Empty;

    [ObservableProperty]
    private float syncedManualSpeed;

    [ObservableProperty]
    private float syncedMovePoint;

    [ObservableProperty]
    private bool isManualSpeedDirty;

    [ObservableProperty]
    private bool isMovePointDirty;

    [ObservableProperty]
    private string manualSpeedValidationMessage = string.Empty;

    [ObservableProperty]
    private string movePointValidationMessage = string.Empty;

    public AxisLimitProfile LimitProfile { get; private set; } = AxisLimitProfile.Unbounded;

    public string CurrentPositionText => ManualNumeric.Format(CurrentPosition);

    public string ServoText => IsServoOn ? "Servo ON" : "Servo OFF";

    public bool HasPendingInput => IsManualSpeedDirty || IsMovePointDirty;

    public bool HasManualSpeedValidationMessage => !string.IsNullOrWhiteSpace(ManualSpeedValidationMessage);

    public bool HasMovePointValidationMessage => !string.IsNullOrWhiteSpace(MovePointValidationMessage);

    public bool CanJogNegative => LimitProfile.CanJogNegative(CurrentPosition);

    public bool CanJogPositive => LimitProfile.CanJogPositive(CurrentPosition);

    partial void OnCurrentPositionChanged(float value)
    {
        OnPropertyChanged(nameof(CurrentPositionText));
        OnPropertyChanged(nameof(CanJogNegative));
        OnPropertyChanged(nameof(CanJogPositive));
    }

    partial void OnIsServoOnChanged(bool value)
    {
        OnPropertyChanged(nameof(ServoText));
    }

    partial void OnIsManualSpeedDirtyChanged(bool value)
    {
        OnPropertyChanged(nameof(HasPendingInput));
    }

    partial void OnIsMovePointDirtyChanged(bool value)
    {
        OnPropertyChanged(nameof(HasPendingInput));
    }

    partial void OnManualSpeedValidationMessageChanged(string value)
    {
        OnPropertyChanged(nameof(HasManualSpeedValidationMessage));
    }

    partial void OnMovePointValidationMessageChanged(string value)
    {
        OnPropertyChanged(nameof(HasMovePointValidationMessage));
    }

    partial void OnManualSpeedInputChanged(string value)
    {
        if (_isApplyingPlcInputs)
        {
            return;
        }

        IsManualSpeedDirty = !ManualNumeric.TryParse(value, out var parsedValue) || !ManualNumeric.AreClose(parsedValue, SyncedManualSpeed);
        ValidateManualSpeedInput();
    }

    partial void OnMovePointInputChanged(string value)
    {
        if (_isApplyingPlcInputs)
        {
            return;
        }

        IsMovePointDirty = !ManualNumeric.TryParse(value, out var parsedValue) || !ManualNumeric.AreClose(parsedValue, SyncedMovePoint);
        ValidateMovePointInput();
    }

    public void ApplyLimitProfile(AxisLimitProfile limitProfile)
    {
        LimitProfile = limitProfile ?? AxisLimitProfile.Unbounded;
        OnPropertyChanged(nameof(CanJogNegative));
        OnPropertyChanged(nameof(CanJogPositive));
        ValidateManualSpeedInput();
        ValidateMovePointInput();
    }

    public void ApplyObservedValues(
        float currentPosition,
        float manualSpeed,
        float movePoint,
        bool isNegativeJogActive,
        bool isPositiveJogActive,
        bool isHomeCommandActive,
        bool isMoveToPointCommandActive,
        bool isHoming,
        bool isHomed,
        bool isNegativeLimitActive,
        bool isPositiveLimitActive,
        bool isServoOn)
    {
        CurrentPosition = currentPosition;
        IsNegativeJogActive = isNegativeJogActive;
        IsPositiveJogActive = isPositiveJogActive;
        IsHomeCommandActive = isHomeCommandActive;
        IsMoveToPointCommandActive = isMoveToPointCommandActive;
        IsHoming = isHoming;
        IsHomed = isHomed;
        IsNegativeLimitActive = isNegativeLimitActive;
        IsPositiveLimitActive = isPositiveLimitActive;
        IsServoOn = isServoOn;

        SyncedManualSpeed = manualSpeed;
        SyncedMovePoint = movePoint;

        SyncInput(nameof(ManualSpeedInput), ManualNumeric.Format(manualSpeed), manualSpeed, isSpeedInput: true);
        SyncInput(nameof(MovePointInput), ManualNumeric.Format(movePoint), movePoint, isSpeedInput: false);
    }

    public void MarkManualSpeedApplied(float value)
    {
        SyncedManualSpeed = value;
        SetInput(nameof(ManualSpeedInput), ManualNumeric.Format(value));
        IsManualSpeedDirty = false;
        ManualSpeedValidationMessage = string.Empty;
    }

    public void MarkMovePointApplied(float value)
    {
        SyncedMovePoint = value;
        SetInput(nameof(MovePointInput), ManualNumeric.Format(value));
        IsMovePointDirty = false;
        MovePointValidationMessage = string.Empty;
    }

    public bool TryGetValidatedManualSpeed(out float speedValue, out string errorMessage)
    {
        if (!ManualNumeric.TryParse(ManualSpeedInput, out speedValue))
        {
            errorMessage = $"Giá trị tốc độ của {DisplayName} không hợp lệ.";
            ManualSpeedValidationMessage = errorMessage;
            return false;
        }

        if (!LimitProfile.TryValidateSpeed(speedValue, out errorMessage))
        {
            errorMessage = $"{DisplayName}: {errorMessage}";
            ManualSpeedValidationMessage = errorMessage;
            return false;
        }

        ManualSpeedValidationMessage = string.Empty;
        return true;
    }

    public bool TryGetValidatedMovePoint(out float movePointValue, out string errorMessage)
    {
        if (!ManualNumeric.TryParse(MovePointInput, out movePointValue))
        {
            errorMessage = $"Giá trị điểm chạy của {DisplayName} không hợp lệ.";
            MovePointValidationMessage = errorMessage;
            return false;
        }

        if (!LimitProfile.TryValidatePosition(movePointValue, out errorMessage))
        {
            errorMessage = $"{DisplayName}: {errorMessage}";
            MovePointValidationMessage = errorMessage;
            return false;
        }

        MovePointValidationMessage = string.Empty;
        return true;
    }

    private void SetInput(string propertyName, string value)
    {
        _isApplyingPlcInputs = true;

        if (propertyName == nameof(ManualSpeedInput))
        {
            ManualSpeedInput = value;
        }
        else
        {
            MovePointInput = value;
        }

        _isApplyingPlcInputs = false;
    }

    private void SyncInput(string propertyName, string nextValue, float syncedValue, bool isSpeedInput)
    {
        var isDirty = isSpeedInput ? IsManualSpeedDirty : IsMovePointDirty;
        if (isDirty)
        {
            var currentInput = isSpeedInput ? ManualSpeedInput : MovePointInput;
            if (ManualNumeric.TryParse(currentInput, out var parsedValue) && ManualNumeric.AreClose(parsedValue, syncedValue))
            {
                if (isSpeedInput)
                {
                    IsManualSpeedDirty = false;
                }
                else
                {
                    IsMovePointDirty = false;
                }
            }
            else
            {
                return;
            }
        }

        SetInput(propertyName, nextValue);
    }

    private void ValidateManualSpeedInput()
    {
        if (_isApplyingPlcInputs)
        {
            return;
        }

        _ = TryGetValidatedManualSpeed(out _, out _);
    }

    private void ValidateMovePointInput()
    {
        if (_isApplyingPlcInputs)
        {
            return;
        }

        _ = TryGetValidatedMovePoint(out _, out _);
    }
}
