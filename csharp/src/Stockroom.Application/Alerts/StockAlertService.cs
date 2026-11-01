using Stockroom.Application.Abstractions;
using Stockroom.Domain.Entities;

namespace Stockroom.Application.Alerts;

public sealed class StockAlertService(IProductRepository products, IStockAlertNotifier notifier) : IStockAlertService
{
    public async Task<IReadOnlyList<StockAlert>> ListAsync(CancellationToken cancellationToken)
    {
        var active = await products.ListAsync(activeOnly: true, cancellationToken);
        return active.Where(p => p.NeedsReorder).Select(StockAlert.From).ToList();
    }

    public async Task CheckAsync(IEnumerable<Product> changed, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(changed);

        var alerts = changed.Where(p => p.NeedsReorder).Select(StockAlert.From).ToList();
        if (alerts.Count > 0)
        {
            await notifier.NotifyAsync(alerts, cancellationToken);
        }
    }
}
