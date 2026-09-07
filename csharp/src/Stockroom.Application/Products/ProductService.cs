using Microsoft.Extensions.Logging;
using Stockroom.Business.Abstractions;
using Stockroom.Business.Exceptions;
using Stockroom.Business.Products;
using Stockroom.Domain.Entities;

namespace Stockroom.Application.Products;

public sealed class ProductService(
    IProductRepository products,
    IProductCatalog catalog,
    IUnitOfWork unitOfWork,
    IClock clock,
    ILogger<ProductService> logger) : IProductService
{
    public async Task<IReadOnlyList<ProductDto>> ListAsync(bool activeOnly, CancellationToken cancellationToken)
    {
        var items = await products.ListAsync(activeOnly, cancellationToken);
        return items.Select(ProductDto.From).ToList();
    }

    public async Task<IReadOnlyList<ProductDto>> ListLowStockAsync(int threshold, CancellationToken cancellationToken)
    {
        ProductRequestValidator.ValidateThreshold(threshold);

        var items = await products.ListLowStockAsync(threshold, cancellationToken);
        return items.Select(ProductDto.From).ToList();
    }

    public async Task<ProductDto> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await GetRequiredAsync(id, cancellationToken);
        return ProductDto.From(product);
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        ProductRequestValidator.Validate(request);

        var product = await catalog.RegisterAsync(request.Sku, request.Name, request.UnitPrice, request.InitialStock, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.ProductCreated(product.Sku, product.Id);
        return ProductDto.From(product);
    }

    public async Task<ProductDto> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        ProductRequestValidator.Validate(request);

        var product = await GetRequiredAsync(id, cancellationToken);
        var now = clock.UtcNow;
        product.Rename(request.Name, now);
        product.ChangePrice(request.UnitPrice, now);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ProductDto.From(product);
    }

    public async Task<ProductDto> RestockAsync(Guid id, RestockRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        ProductRequestValidator.Validate(request);

        var product = await GetRequiredAsync(id, cancellationToken);
        product.Restock(request.Quantity, clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.ProductRestocked(product.Sku, request.Quantity, product.QuantityOnHand);
        return ProductDto.From(product);
    }

    public async Task DiscontinueAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await GetRequiredAsync(id, cancellationToken);
        product.Discontinue(clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.ProductDiscontinued(product.Sku);
    }

    private async Task<Product> GetRequiredAsync(Guid id, CancellationToken cancellationToken)
    {
        return await products.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), id);
    }
}
