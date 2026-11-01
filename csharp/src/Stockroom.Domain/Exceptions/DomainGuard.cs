using System.Runtime.CompilerServices;

namespace Stockroom.Domain.Exceptions;

/// <summary>
/// Argument checks that fail as a <see cref="DomainException"/>, so a rule broken inside the domain
/// reaches the caller as a business-rule violation rather than as a server error.
/// </summary>
public static class DomainGuard
{
    public static void Positive(int value, [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        if (value <= 0)
        {
            throw new DomainException($"{name} must be greater than zero, got {value}.");
        }
    }

    public static void NotNegative(int value, [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        if (value < 0)
        {
            throw new DomainException($"{name} must not be negative, got {value}.");
        }
    }
}
