using Stockroom.Application.Common;

namespace Stockroom.Application.Products;

internal static class ProductRequestValidator
{
    private const int MaxSkuLength = 32;
    private const int MaxNameLength = 200;

    public static void Validate(CreateProductRequest request)
    {
        new ValidationErrors()
            .RequireNotBlank(nameof(request.Sku), request.Sku)
            .RequireMaxLength(nameof(request.Sku), request.Sku, MaxSkuLength)
            .RequireNotBlank(nameof(request.Name), request.Name)
            .RequireMaxLength(nameof(request.Name), request.Name, MaxNameLength)
            .RequireNonNegative(nameof(request.UnitPrice), request.UnitPrice)
            .RequireNonNegative(nameof(request.InitialStock), request.InitialStock)
            .ThrowIfAny();
    }

    public static void Validate(UpdateProductRequest request)
    {
        new ValidationErrors()
            .RequireNotBlank(nameof(request.Name), request.Name)
            .RequireMaxLength(nameof(request.Name), request.Name, MaxNameLength)
            .RequireNonNegative(nameof(request.UnitPrice), request.UnitPrice)
            .ThrowIfAny();
    }

    public static void Validate(RestockRequest request)
    {
        new ValidationErrors()
            .RequirePositive(nameof(request.Quantity), request.Quantity)
            .ThrowIfAny();
    }

    public static void ValidateThreshold(int threshold)
    {
        new ValidationErrors()
            .RequireNonNegative(nameof(threshold), threshold)
            .ThrowIfAny();
    }
}
