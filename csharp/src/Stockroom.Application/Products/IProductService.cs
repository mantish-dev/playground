namespace Stockroom.Application.Products;

public interface IProductService
{
    Task<IReadOnlyList<ProductDto>> ListAsync(bool activeOnly, CancellationToken cancellationToken);

    Task<IReadOnlyList<ProductDto>> ListLowStockAsync(int threshold, CancellationToken cancellationToken);

    Task<ProductDto> GetAsync(Guid id, CancellationToken cancellationToken);

    Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken);

    Task<ProductDto> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken);

    Task<ProductDto> RestockAsync(Guid id, RestockRequest request, CancellationToken cancellationToken);

    Task DiscontinueAsync(Guid id, CancellationToken cancellationToken);
}
