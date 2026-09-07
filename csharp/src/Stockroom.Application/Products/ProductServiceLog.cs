using Microsoft.Extensions.Logging;

namespace Stockroom.Application.Products;

internal static partial class ProductServiceLog
{
    [LoggerMessage(EventId = 1000, Level = LogLevel.Information, Message = "Created product {Sku} ({ProductId})")]
    public static partial void ProductCreated(this ILogger logger, string sku, Guid productId);

    [LoggerMessage(EventId = 1001, Level = LogLevel.Information, Message = "Restocked {Sku} by {Quantity}, now {OnHand} on hand")]
    public static partial void ProductRestocked(this ILogger logger, string sku, int quantity, int onHand);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Information, Message = "Discontinued product {Sku}")]
    public static partial void ProductDiscontinued(this ILogger logger, string sku);
}
