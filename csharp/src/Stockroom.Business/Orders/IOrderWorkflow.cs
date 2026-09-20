using Stockroom.Domain.Entities;

namespace Stockroom.Business.Orders;

/// <summary>
/// The order lifecycle: placing reserves stock, shipping consumes it, cancelling releases it.
/// Callers persist the resulting state through <see cref="Abstractions.IUnitOfWork"/>.
/// </summary>
public interface IOrderWorkflow
{
    Task<Order> PlaceAsync(string customerEmail, IReadOnlyList<OrderRequestLine> lines, CancellationToken cancellationToken);

    Task ShipAsync(Order order, CancellationToken cancellationToken);

    Task CancelAsync(Order order, CancellationToken cancellationToken);
}

public readonly record struct OrderRequestLine(Guid ProductId, int Quantity);
