using Stockroom.Application.Abstractions;

namespace Stockroom.Tests.Fakes;

internal sealed class FixedClock(DateTimeOffset utcNow) : IClock
{
    public static readonly DateTimeOffset Default = new(2026, 1, 15, 10, 30, 0, TimeSpan.Zero);

    public DateTimeOffset UtcNow { get; set; } = utcNow;

    public void Advance(TimeSpan by) => UtcNow += by;
}
