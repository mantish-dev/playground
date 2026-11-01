using Microsoft.Extensions.Logging;
using Stockroom.Application.Alerts;

namespace Stockroom.Infrastructure.Alerts;

/// <summary>
/// Writes each alert to the log as a warning, where the warehouse's log alerting picks it up. The
/// notifier to replace once alerts go out by e-mail or chat.
/// </summary>
internal sealed class LoggingStockAlertNotifier(ILogger<LoggingStockAlertNotifier> logger) : IStockAlertNotifier
{
    public Task NotifyAsync(IReadOnlyList<StockAlert> alerts, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(alerts);

        foreach (var alert in alerts)
        {
            logger.StockBelowThreshold(alert.Sku, alert.QuantityAvailable, alert.ReorderThreshold);
        }

        return Task.CompletedTask;
    }
}

internal static partial class LoggingStockAlertNotifierLog
{
    [LoggerMessage(EventId = 4000, Level = LogLevel.Warning, Message = "Stock of {Sku} is down to {Available}, at or below its reorder threshold of {ReorderThreshold}")]
    public static partial void StockBelowThreshold(this ILogger logger, string sku, int available, int reorderThreshold);
}
