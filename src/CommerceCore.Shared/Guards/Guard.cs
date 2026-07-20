using CommerceCore.Shared.Exceptions;

namespace CommerceCore.Shared.Guards;

/// <summary>Fluent, allocation-light guard clauses used throughout Domain entities
/// and Application command handlers to fail fast on invalid state.</summary>
public static class Guard
{
    public static string AgainstNullOrWhiteSpace(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"'{paramName}' cannot be null or whitespace.", paramName);
        return value;
    }

    public static T AgainstNull<T>(T? value, string paramName) where T : class
    {
        if (value is null)
            throw new ArgumentNullException(paramName);
        return value;
    }

    public static Guid AgainstEmpty(Guid value, string paramName)
    {
        if (value == Guid.Empty)
            throw new ArgumentException($"'{paramName}' cannot be an empty GUID.", paramName);
        return value;
    }

    public static decimal AgainstNegative(decimal value, string paramName)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(paramName, "Value cannot be negative.");
        return value;
    }

    public static int AgainstNegativeOrZero(int value, string paramName)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(paramName, "Value must be greater than zero.");
        return value;
    }

    public static void AgainstCondition(bool condition, string message)
    {
        if (condition)
            throw new BusinessRuleException(message);
    }
}
