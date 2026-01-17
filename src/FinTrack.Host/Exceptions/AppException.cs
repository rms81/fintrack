namespace FinTrack.Host.Exceptions;

/// <summary>
/// Base exception for application-specific errors.
/// </summary>
public abstract class AppException : Exception
{
    public abstract int StatusCode { get; }
    public abstract string ErrorType { get; }

    protected AppException(string message) : base(message) { }
    protected AppException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Thrown when a requested resource is not found.
/// </summary>
public class NotFoundException : AppException
{
    public override int StatusCode => StatusCodes.Status404NotFound;
    public override string ErrorType => "https://fintrack.app/errors/not-found";

    public string ResourceType { get; }
    public string? ResourceId { get; }

    public NotFoundException(string resourceType, string? resourceId = null)
        : base($"{resourceType}{(resourceId is not null ? $" with ID '{resourceId}'" : "")} was not found")
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
    }
}

/// <summary>
/// Thrown when request validation fails.
/// </summary>
public class ValidationException : AppException
{
    public override int StatusCode => StatusCodes.Status400BadRequest;
    public override string ErrorType => "https://fintrack.app/errors/validation";

    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(string message) : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation errors occurred")
    {
        Errors = errors;
    }

    public ValidationException(string field, string error)
        : base($"Validation failed for {field}: {error}")
    {
        Errors = new Dictionary<string, string[]> { { field, new[] { error } } };
    }
}

/// <summary>
/// Thrown when an operation conflicts with existing state.
/// </summary>
public class ConflictException : AppException
{
    public override int StatusCode => StatusCodes.Status409Conflict;
    public override string ErrorType => "https://fintrack.app/errors/conflict";

    public ConflictException(string message) : base(message) { }
}

/// <summary>
/// Thrown when user is not authorized to access a resource.
/// </summary>
public class ForbiddenException : AppException
{
    public override int StatusCode => StatusCodes.Status403Forbidden;
    public override string ErrorType => "https://fintrack.app/errors/forbidden";

    public ForbiddenException(string message = "You do not have permission to access this resource")
        : base(message) { }
}
