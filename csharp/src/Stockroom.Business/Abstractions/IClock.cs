namespace Stockroom.Business.Abstractions;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
