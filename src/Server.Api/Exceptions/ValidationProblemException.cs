namespace Server.Api.Exceptions;

public sealed class ValidationProblemException : Exception
{
    public ValidationProblemException(IDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }

    public IDictionary<string, string[]> Errors { get; }
}
