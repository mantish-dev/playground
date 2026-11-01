using Stockroom.Domain.Entities;

namespace Stockroom.Application.Alerts;

public interface IStockAlertService
{
    Task<IReadOnlyList<StockAlert>> ListAsync(CancellationToken cancellationToken);

    /// <summary>Notifies about every product in <paramref name="changed"/> that now needs reordering.</summary>
    Task CheckAsync(IEnumerable<Product> changed, CancellationToken cancellationToken);
}
