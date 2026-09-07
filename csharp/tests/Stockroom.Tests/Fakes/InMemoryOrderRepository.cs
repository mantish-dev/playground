using Stockroom.Business.Abstractions;
using Stockroom.Domain.Entities;

namespace Stockroom.Tests.Fakes;

internal sealed class InMemoryOrderRepository : IOrderRepository
{
    private readonly Dictionary<Guid, Order> _orders = [];

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return Task.FromResult(_orders.GetValueOrDefault(id));
    }

    public Task<IReadOnlyList<Order>> ListAsync(OrderStatus? status, CancellationToken cancellationToken)
    {
        IReadOnlyList<Order> items = _orders.Values
            .Where(o => status is null || o.Status == status)
            .OrderByDescending(o => o.PlacedAt)
            .ToList();

        return Task.FromResult(items);
    }

    public void Add(Order order)
    {
        _orders[order.Id] = order;
    }
}
