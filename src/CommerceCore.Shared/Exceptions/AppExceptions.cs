namespace CommerceCore.Shared.Exceptions;

/// <summary>Base type for all handled application exceptions. Caught by the global
/// exception middleware and translated into an RFC 7807 ProblemDetails response.</summary>
public abstract class AppException : Exception
{
    public abstract int StatusCode { get; }
    protected AppException(string message) : base(message) { }
}

public sealed class NotFoundException : AppException
{
    public override int StatusCode => 404;
    public NotFoundException(string entityName, object key)
        : base($"{entityName} with identifier '{key}' was not found.") { }
    public NotFoundException(string message) : base(message) { }
}

public sealed class ValidationAppException : AppException
{
    public override int StatusCode => 400;
    public IDictionary<string, string[]> Errors { get; }

    public ValidationAppException(IDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }
}

public sealed class ConflictException : AppException
{
    public override int StatusCode => 409;
    public ConflictException(string message) : base(message) { }
}

public sealed class UnauthorizedAppException : AppException
{
    public override int StatusCode => 401;
    public UnauthorizedAppException(string message = "Authentication is required.") : base(message) { }
}

public sealed class ForbiddenAppException : AppException
{
    public override int StatusCode => 403;
    public ForbiddenAppException(string message = "You do not have permission to perform this action.") : base(message) { }
}

public sealed class BusinessRuleException : AppException
{
    public override int StatusCode => 422;
    public BusinessRuleException(string message) : base(message) { }
}
