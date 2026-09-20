using Stockroom.Business.Exceptions;

namespace Stockroom.Business.Validation;

/// <summary>
/// Collects field-level validation failures and raises a single <see cref="ValidationException"/>.
/// </summary>
public sealed class ValidationErrors
{
    private readonly Dictionary<string, List<string>> _errors = new(StringComparer.Ordinal);

    public bool HasErrors => _errors.Count > 0;

    public ValidationErrors Add(string field, string message)
    {
        if (!_errors.TryGetValue(field, out var messages))
        {
            messages = [];
            _errors[field] = messages;
        }

        messages.Add(message);
        return this;
    }

    public ValidationErrors RequireNotBlank(string field, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            Add(field, $"{field} is required.");
        }

        return this;
    }

    public ValidationErrors RequireMaxLength(string field, string? value, int maxLength)
    {
        if (value is not null && value.Length > maxLength)
        {
            Add(field, $"{field} must be at most {maxLength} characters.");
        }

        return this;
    }

    public ValidationErrors RequireNonNegative(string field, decimal value)
    {
        if (value < 0)
        {
            Add(field, $"{field} must not be negative.");
        }

        return this;
    }

    public ValidationErrors RequirePositive(string field, int value)
    {
        if (value <= 0)
        {
            Add(field, $"{field} must be greater than zero.");
        }

        return this;
    }

    public void ThrowIfAny()
    {
        if (HasErrors)
        {
            throw new ValidationException(_errors.ToDictionary(kv => kv.Key, kv => kv.Value.ToArray(), StringComparer.Ordinal));
        }
    }
}
