using Microsoft.EntityFrameworkCore;
using Stockroom.Application.Abstractions;
using Stockroom.Domain.Entities;

namespace Stockroom.Infrastructure.Persistence.Repositories;

internal sealed class OrderRepository(StockroomDbContext dbContext) : IOrderRepository
{
    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Orders.SingleOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> ListAsync(OrderStatus? status, CancellationToken cancellationToken)
    {
        IQueryable<Order> query = dbContext.Orders;
        if (status is { } s)
        {
            query = query.Where(o => o.Status == s);
        }

        return await query
            .OrderByDescending(o => o.PlacedAt)
            .ToListAsync(cancellationToken);
    }

    public void Add(Order order)
    {
        dbContext.Orders.Add(order);
    }
}
