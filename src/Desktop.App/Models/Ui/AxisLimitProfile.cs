namespace Desktop.App.Models.Ui;

public sealed record AxisLimitProfile(float? SpeedLimit, float? NegativeLimit, float? PositiveLimit)
{
    private const float Tolerance = 0.0001f;

    public static AxisLimitProfile Unbounded { get; } = new(null, null, null);

    public bool CanJogNegative(float currentPosition)
        => !NegativeLimit.HasValue || currentPosition > NegativeLimit.Value + Tolerance;

    public bool CanJogPositive(float currentPosition)
        => !PositiveLimit.HasValue || currentPosition < PositiveLimit.Value - Tolerance;

    public bool TryValidateSpeed(float value, out string errorMessage)
    {
        if (value < 0f)
        {
            errorMessage = "Tốc độ phải lớn hơn hoặc bằng 0.";
            return false;
        }

        if (SpeedLimit.HasValue && value > SpeedLimit.Value + Tolerance)
        {
            errorMessage = $"Tốc độ phải nhỏ hơn hoặc bằng {ManualNumeric.Format(SpeedLimit.Value)} mm/s.";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }

    public bool TryValidatePosition(float value, out string errorMessage)
    {
        if (NegativeLimit.HasValue && value < NegativeLimit.Value - Tolerance)
        {
            errorMessage = $"Giá trị phải nằm trong {DescribePositionRange()}.";
            return false;
        }

        if (PositiveLimit.HasValue && value > PositiveLimit.Value + Tolerance)
        {
            errorMessage = $"Giá trị phải nằm trong {DescribePositionRange()}.";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }

    public string DescribePositionRange()
    {
        var minText = NegativeLimit.HasValue ? ManualNumeric.Format(NegativeLimit.Value) : "-inf";
        var maxText = PositiveLimit.HasValue ? ManualNumeric.Format(PositiveLimit.Value) : "+inf";
        return $"[{minText}; {maxText}] mm";
    }
}
