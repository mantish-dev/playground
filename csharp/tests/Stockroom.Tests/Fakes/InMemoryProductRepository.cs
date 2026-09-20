using Stockroom.Application.Abstractions;
using Stockroom.Domain.Entities;

namespace Stockroom.Tests.Fakes;

internal sealed class InMemoryProductRepository : IProductRepository
{
    private readonly Dictionary<Guid, Product> _products = [];

    public IReadOnlyCollection<Product> All => _products.Values;

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return Task.FromResult(_products.GetValueOrDefault(id));
    }

    public Task<IReadOnlyList<Product>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken)
    {
        IReadOnlyList<Product> found = ids.Where(_products.ContainsKey).Select(id => _products[id]).ToList();
        return Task.FromResult(found);
    }

    public Task<bool> ExistsBySkuAsync(string sku, CancellationToken cancellationToken)
    {
        return Task.FromResult(_products.Values.Any(p => p.Sku == sku));
    }

    public Task<IReadOnlyList<Product>> ListAsync(bool activeOnly, CancellationToken cancellationToken)
    {
        IReadOnlyList<Product> items = _products.Values
            .Where(p => !activeOnly || !p.IsDiscontinued)
            .OrderBy(p => p.Sku, StringComparer.Ordinal)
            .ToList();
        return Task.FromResult(items);
    }

    public Task<IReadOnlyList<Product>> ListLowStockAsync(int threshold, CancellationToken cancellationToken)
    {
        IReadOnlyList<Product> items = _products.Values
            .Where(p => !p.IsDiscontinued && p.QuantityAvailable <= threshold)
            .OrderBy(p => p.QuantityAvailable)
            .ThenBy(p => p.Sku, StringComparer.Ordinal)
            .ToList();
        return Task.FromResult(items);
    }

    public void Add(Product product)
    {
        _products[product.Id] = product;
    }
}
