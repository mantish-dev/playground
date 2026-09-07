using Stockroom.Domain.Entities;

namespace Stockroom.Application.Orders;

public interface IOrderService
{
    Task<IReadOnlyList<OrderDto>> ListAsync(OrderStatus? status, CancellationToken cancellationToken);

    Task<OrderDto> GetAsync(Guid id, CancellationToken cancellationToken);

    Task<OrderDto> PlaceAsync(PlaceOrderRequest request, CancellationToken cancellationToken);

    Task<OrderDto> ShipAsync(Guid id, CancellationToken cancellationToken);

    Task<OrderDto> CancelAsync(Guid id, CancellationToken cancellationToken);
}
