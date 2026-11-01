namespace Stockroom.Application.Alerts;

/// <summary>Tells whoever restocks the warehouse that products need reordering.</summary>
public interface IStockAlertNotifier
{
    Task NotifyAsync(IReadOnlyList<StockAlert> alerts, CancellationToken cancellationToken);
}
