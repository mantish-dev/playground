using Microsoft.EntityFrameworkCore;
using Stockroom.Application.Abstractions;
using Stockroom.Domain.Entities;

namespace Stockroom.Infrastructure.Persistence.Repositories;

internal sealed class ProductRepository(StockroomDbContext dbContext) : IProductRepository
{
    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Products.SingleOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken)
    {
        return await dbContext.Products
            .Where(p => ids.Contains(p.Id))
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsBySkuAsync(string sku, CancellationToken cancellationToken)
    {
        return dbContext.Products.AnyAsync(p => p.Sku == sku, cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> ListAsync(bool activeOnly, CancellationToken cancellationToken)
    {
        IQueryable<Product> query = dbContext.Products;
        if (activeOnly)
        {
            query = query.Where(p => !p.IsDiscontinued);
        }

        return await query
            .OrderBy(p => p.Sku)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> ListLowStockAsync(int threshold, CancellationToken cancellationToken)
    {
        return await dbContext.Products
            .Where(p => !p.IsDiscontinued)
            .Where(p => p.QuantityOnHand - p.QuantityReserved <= threshold)
            .OrderBy(p => p.QuantityOnHand - p.QuantityReserved)
            .ThenBy(p => p.Sku)
            .ToListAsync(cancellationToken);
    }

    public void Add(Product product)
    {
        dbContext.Products.Add(product);
    }
}
