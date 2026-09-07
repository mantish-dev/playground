namespace Stockroom.Application.Abstractions;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
