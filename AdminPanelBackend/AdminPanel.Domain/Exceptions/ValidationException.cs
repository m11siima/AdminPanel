namespace AdminPanel.Domain.Exceptions;

public class ValidationException : DomainException
{
    public Dictionary<string, string[]> Errors { get; }

    public ValidationException(string message) : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(string message, Dictionary<string, string[]> errors) : base(message)
    {
        Errors = errors;
    }

    public ValidationException(string property, string error) : base($"Validation failed for '{property}': {error}")
    {
        Errors = new Dictionary<string, string[]>
        {
            { property, new[] { error } }
        };
    }
}

