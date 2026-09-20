using Stockroom.Domain.Entities;

namespace Stockroom.Business.Products;

/// <summary>
/// Owns the rules for what may enter the catalogue: SKU normalisation and uniqueness.
/// </summary>
public interface IProductCatalog
{
    Task<Product> RegisterAsync(string sku, string name, decimal unitPrice, int initialStock, CancellationToken cancellationToken);
}
