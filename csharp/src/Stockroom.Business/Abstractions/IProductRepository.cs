using Stockroom.Domain.Entities;

namespace Stockroom.Business.Abstractions;

public interface IProductRepository
{
    /// <summary>Returns the product with the given id, or <c>null</c> when it does not exist.</summary>
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Returns every product whose id is in <paramref name="ids"/>. Unknown ids are skipped.</summary>
    Task<IReadOnlyList<Product>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken);

    /// <summary>Checks whether a product with the given (already normalised) SKU exists.</summary>
    Task<bool> ExistsBySkuAsync(string sku, CancellationToken cancellationToken);

    /// <summary>Lists products ordered by SKU, optionally hiding discontinued ones.</summary>
    Task<IReadOnlyList<Product>> ListAsync(bool activeOnly, CancellationToken cancellationToken);

    /// <summary>Lists active products whose available quantity is at or below <paramref name="threshold"/>.</summary>
    Task<IReadOnlyList<Product>> ListLowStockAsync(int threshold, CancellationToken cancellationToken);

    /// <summary>Tracks a new product; it is persisted on the next <see cref="IUnitOfWork.SaveChangesAsync"/>.</summary>
    void Add(Product product);
}
