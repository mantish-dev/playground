using Microsoft.Extensions.DependencyInjection;
using Stockroom.Application.Alerts;
using Stockroom.Application.Orders;
using Stockroom.Application.Products;

namespace Stockroom.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IStockAlertService, StockAlertService>();
        return services;
    }
}
