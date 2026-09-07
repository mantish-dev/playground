using Microsoft.Extensions.Logging;

namespace Stockroom.Application.Orders;

internal static partial class OrderServiceLog
{
    [LoggerMessage(EventId = 2000, Level = LogLevel.Information, Message = "Placed order {OrderId} for {Customer} with {LineCount} lines totalling {Total}")]
    public static partial void OrderPlaced(this ILogger logger, Guid orderId, string customer, int lineCount, decimal total);

    [LoggerMessage(EventId = 2001, Level = LogLevel.Information, Message = "Shipped order {OrderId}")]
    public static partial void OrderShipped(this ILogger logger, Guid orderId);

    [LoggerMessage(EventId = 2002, Level = LogLevel.Information, Message = "Cancelled order {OrderId}")]
    public static partial void OrderCancelled(this ILogger logger, Guid orderId);
}
