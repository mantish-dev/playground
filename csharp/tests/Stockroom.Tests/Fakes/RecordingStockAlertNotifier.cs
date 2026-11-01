using Stockroom.Application.Alerts;

namespace Stockroom.Tests.Fakes;

internal sealed class RecordingStockAlertNotifier : IStockAlertNotifier
{
    private readonly List<IReadOnlyList<StockAlert>> _batches = [];

    public IReadOnlyList<IReadOnlyList<StockAlert>> Batches => _batches;

    public Task NotifyAsync(IReadOnlyList<StockAlert> alerts, CancellationToken cancellationToken)
    {
        _batches.Add(alerts);
        return Task.CompletedTask;
    }
}
