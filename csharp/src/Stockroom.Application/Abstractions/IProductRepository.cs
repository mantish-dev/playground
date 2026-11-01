using Stockroom.Domain.Entities;

namespace Stockroom.Application.Abstractions;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Product>> FindManyAsync(IReadOnlyCollection<Guid> productIds, CancellationToken cancellationToken);

    Task<bool> ExistsBySkuAsync(string sku, CancellationToken cancellationToken);

    Task<IReadOnlyList<Product>> ListAsync(bool activeOnly, CancellationToken cancellationToken);

    void Add(Product product);
}
