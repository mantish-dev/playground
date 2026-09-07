using Stockroom.Domain.Entities;

namespace Stockroom.Business.Abstractions;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Order>> ListAsync(OrderStatus? status, CancellationToken cancellationToken);

    void Add(Order order);
}
