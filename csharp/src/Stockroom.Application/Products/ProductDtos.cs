using Stockroom.Domain.Entities;

namespace Stockroom.Application.Products;

public sealed record ProductDto(
    Guid Id,
    string Sku,
    string Name,
    decimal UnitPrice,
    int QuantityOnHand,
    int QuantityReserved,
    int QuantityAvailable,
    bool IsDiscontinued,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public static ProductDto From(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);

        return new ProductDto(
            product.Id,
            product.Sku,
            product.Name,
            product.UnitPrice,
            product.QuantityOnHand,
            product.QuantityReserved,
            product.QuantityAvailable,
            product.IsDiscontinued,
            product.CreatedAt,
            product.UpdatedAt);
    }
}

public sealed record CreateProductRequest(string Sku, string Name, decimal UnitPrice, int InitialStock);

public sealed record UpdateProductRequest(string Name, decimal UnitPrice);

public sealed record RestockRequest(int Quantity);
