using Microsoft.Extensions.DependencyInjection;
using Stockroom.Business.Orders;
using Stockroom.Business.Products;

namespace Stockroom.Business;

public static class DependencyInjection
{
    public static IServiceCollection AddBusiness(this IServiceCollection services)
    {
        services.AddScoped<IOrderWorkflow, OrderWorkflow>();
        services.AddScoped<IProductCatalog, ProductCatalog>();
        return services;
    }
}
