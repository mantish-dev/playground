using Stockroom.Business.Abstractions;
using Stockroom.Business.Exceptions;
using Stockroom.Domain.Entities;

namespace Stockroom.Business.Products;

internal sealed class ProductCatalog(IProductRepository products, IClock clock) : IProductCatalog
{
    public async Task<Product> RegisterAsync(string sku, string name, decimal unitPrice, int initialStock, CancellationToken cancellationToken)
    {
        var normalisedSku = NormaliseSku(sku);
        if (await products.ExistsBySkuAsync(normalisedSku, cancellationToken))
        {
            throw new ConflictException($"A product with SKU {normalisedSku} already exists.");
        }

        var product = Product.Create(normalisedSku, name, unitPrice, initialStock, clock.UtcNow);
        products.Add(product);
        return product;
    }

    private static string NormaliseSku(string sku)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        return sku.Trim().ToUpperInvariant();
    }
}
