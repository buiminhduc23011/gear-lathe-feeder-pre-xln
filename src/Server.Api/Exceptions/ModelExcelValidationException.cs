namespace Server.Api.Exceptions;

public sealed class ModelExcelValidationException : Exception
{
    public const string ErrorCode = "MODEL_EXCEL_VALIDATION_FAILED";
    public const string ErrorMessage = "File Excel có dữ liệu model không hợp lệ.";

    public ModelExcelValidationException(IReadOnlyList<ModelExcelValidationError> errors)
        : base(ErrorMessage)
    {
        Errors = errors;
    }

    public IReadOnlyList<ModelExcelValidationError> Errors { get; }
}

public sealed class ModelExcelValidationError
{
    public string? Sheet { get; init; }
    public int? Row { get; init; }
    public string? Column { get; init; }
    public string? ArticleId { get; init; }
    public string Field { get; init; } = string.Empty;
    public string? Value { get; init; }
    public string Message { get; init; } = string.Empty;
    public IReadOnlyList<ModelExcelValidationConflictValue>? Values { get; init; }
}

public sealed class ModelExcelValidationConflictValue
{
    public string Sheet { get; init; } = string.Empty;
    public int Row { get; init; }
    public string Column { get; init; } = string.Empty;
    public string? Value { get; init; }
    public string? NormalizedValue { get; init; }
}
