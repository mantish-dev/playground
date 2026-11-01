using Stockroom.Domain.Entities;

namespace Stockroom.Application.Alerts;

/// <summary>A product whose available stock has fallen to or below its reorder threshold.</summary>
public sealed record StockAlert(Guid ProductId, string Sku, string Name, int QuantityAvailable, int ReorderThreshold)
{
    public static StockAlert From(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);
        return new StockAlert(product.Id, product.Sku, product.Name, product.QuantityAvailable, product.ReorderThreshold);
    }
}
