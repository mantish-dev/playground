using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Stockroom.Business.Abstractions;
using Stockroom.Infrastructure.Persistence;
using Stockroom.Infrastructure.Persistence.Repositories;
using Stockroom.Infrastructure.Time;

namespace Stockroom.Infrastructure;

public static class DependencyInjection
{
    public const string ConnectionStringName = "Stockroom";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException($"Connection string '{ConnectionStringName}' is not configured.");

        services.AddDbContext<StockroomDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<StockroomDbContext>());
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddHostedService<DatabaseInitializer>();

        return services;
    }
}
