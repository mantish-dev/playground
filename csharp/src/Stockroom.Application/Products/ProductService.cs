using Microsoft.Extensions.Logging;
using Stockroom.Application.Abstractions;
using Stockroom.Application.Common;
using Stockroom.Domain.Entities;

namespace Stockroom.Application.Products;

public sealed class ProductService(
    IProductRepository products,
    IUnitOfWork unitOfWork,
    IClock clock,
    ILogger<ProductService> logger) : IProductService
{
    private const int MaxSkuLength = 32;
    private const int MaxNameLength = 200;

    public async Task<IReadOnlyList<ProductDto>> ListAsync(bool activeOnly, CancellationToken cancellationToken)
    {
        var items = await products.ListAsync(activeOnly, cancellationToken);
        return items.Select(ProductDto.From).ToList();
    }

    public async Task<ProductDto> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await RequireAsync(id, cancellationToken);
        return ProductDto.From(product);
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        new ValidationErrors()
            .RequireNotBlank(nameof(request.Sku), request.Sku)
            .RequireMaxLength(nameof(request.Sku), request.Sku, MaxSkuLength)
            .RequireNotBlank(nameof(request.Name), request.Name)
            .RequireMaxLength(nameof(request.Name), request.Name, MaxNameLength)
            .RequireNonNegative(nameof(request.UnitPrice), request.UnitPrice)
            .RequireNonNegative(nameof(request.InitialStock), request.InitialStock)
            .ThrowIfAny();

        var sku = request.Sku.Trim().ToUpperInvariant();
        if (await products.ExistsBySkuAsync(sku, cancellationToken))
        {
            throw new ConflictException($"A product with SKU {sku} already exists.");
        }

        var product = Product.Create(sku, request.Name, request.UnitPrice, request.InitialStock, clock.UtcNow);
        products.Add(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.ProductCreated(product.Sku, product.Id);
        return ProductDto.From(product);
    }

    public async Task<ProductDto> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        new ValidationErrors()
            .RequireNotBlank(nameof(request.Name), request.Name)
            .RequireMaxLength(nameof(request.Name), request.Name, MaxNameLength)
            .RequireNonNegative(nameof(request.UnitPrice), request.UnitPrice)
            .ThrowIfAny();

        var product = await RequireAsync(id, cancellationToken);
        var now = clock.UtcNow;
        product.Rename(request.Name, now);
        product.ChangePrice(request.UnitPrice, now);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ProductDto.From(product);
    }

    public async Task<ProductDto> RestockAsync(Guid id, RestockRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        new ValidationErrors()
            .RequirePositive(nameof(request.Quantity), request.Quantity)
            .ThrowIfAny();

        var product = await RequireAsync(id, cancellationToken);
        product.Restock(request.Quantity, clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.ProductRestocked(product.Sku, request.Quantity, product.QuantityOnHand);
        return ProductDto.From(product);
    }

    public async Task DiscontinueAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await RequireAsync(id, cancellationToken);
        product.Discontinue(clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.ProductDiscontinued(product.Sku);
    }

    private async Task<Product> RequireAsync(Guid id, CancellationToken cancellationToken)
    {
        return await products.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), id);
    }
}
