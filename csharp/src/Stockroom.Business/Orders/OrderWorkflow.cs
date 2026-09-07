using Stockroom.Business.Abstractions;
using Stockroom.Business.Exceptions;
using Stockroom.Domain.Entities;

namespace Stockroom.Business.Orders;

internal sealed class OrderWorkflow(IProductRepository products, IClock clock) : IOrderWorkflow
{
    public async Task<Order> PlaceAsync(string customerEmail, IReadOnlyList<OrderRequestLine> lines, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(lines);

        var productIds = lines.Select(l => l.ProductId).Distinct().ToList();
        var found = await products.GetByIdsAsync(productIds, cancellationToken);
        var byId = found.ToDictionary(p => p.Id);

        var missing = productIds.Where(id => !byId.ContainsKey(id)).ToList();
        if (missing.Count > 0)
        {
            throw new NotFoundException(nameof(Product), missing[0]);
        }

        var items = lines.Select(l => (byId[l.ProductId], l.Quantity));
        return Order.Place(customerEmail, items, clock.UtcNow);
    }

    public async Task ShipAsync(Order order, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(order);

        var lineProducts = await LoadLineProductsAsync(order, cancellationToken);
        order.Ship(lineProducts, clock.UtcNow);
    }

    public async Task CancelAsync(Order order, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(order);

        var lineProducts = await LoadLineProductsAsync(order, cancellationToken);
        order.Cancel(lineProducts, clock.UtcNow);
    }

    private async Task<IReadOnlyDictionary<Guid, Product>> LoadLineProductsAsync(Order order, CancellationToken cancellationToken)
    {
        var ids = order.Lines.Select(l => l.ProductId).ToList();
        var found = await products.GetByIdsAsync(ids, cancellationToken);
        return found.ToDictionary(p => p.Id);
    }
}
