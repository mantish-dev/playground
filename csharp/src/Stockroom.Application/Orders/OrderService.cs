using Microsoft.Extensions.Logging;
using Stockroom.Application.Abstractions;
using Stockroom.Application.Alerts;
using Stockroom.Application.Common;
using Stockroom.Domain.Entities;

namespace Stockroom.Application.Orders;

public sealed class OrderService(
    IOrderRepository orders,
    IProductRepository products,
    IUnitOfWork unitOfWork,
    IClock clock,
    IStockAlertService stockAlerts,
    ILogger<OrderService> logger) : IOrderService
{
    public async Task<IReadOnlyList<OrderDto>> ListAsync(OrderStatus? status, CancellationToken cancellationToken)
    {
        var items = await orders.ListAsync(status, cancellationToken);
        return items.Select(OrderDto.From).ToList();
    }

    public async Task<OrderDto> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await RequireAsync(id, cancellationToken);
        return OrderDto.From(order);
    }

    public async Task<OrderDto> PlaceAsync(PlaceOrderRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        PlaceOrderRequestValidator.Validate(request);

        var productIds = request.Lines.Select(l => l.ProductId).Distinct().ToList();
        var found = await products.FindManyAsync(productIds, cancellationToken);
        var productsById = found.ToDictionary(p => p.Id);

        var missing = productIds.Where(id => !productsById.ContainsKey(id)).ToList();
        if (missing.Count > 0)
        {
            throw new NotFoundException(nameof(Product), missing[0]);
        }

        var items = request.Lines.Select(l => (productsById[l.ProductId], l.Quantity));
        var order = Order.Place(request.CustomerEmail, items, clock.UtcNow);

        orders.Add(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.OrderPlaced(order.Id, order.CustomerEmail, order.Lines.Count, order.Total);
        return OrderDto.From(order);
    }

    public async Task<OrderDto> ShipAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await RequireAsync(id, cancellationToken);
        var lineProducts = await LoadLineProductsAsync(order, cancellationToken);

        order.Ship(lineProducts, clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await stockAlerts.CheckAsync(lineProducts.Values, cancellationToken);

        logger.OrderShipped(order.Id);
        return OrderDto.From(order);
    }

    public async Task<OrderDto> CancelAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await RequireAsync(id, cancellationToken);
        var lineProducts = await LoadLineProductsAsync(order, cancellationToken);

        order.Cancel(lineProducts, clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.OrderCancelled(order.Id);
        return OrderDto.From(order);
    }

    private async Task<IReadOnlyDictionary<Guid, Product>> LoadLineProductsAsync(Order order, CancellationToken cancellationToken)
    {
        var found = await products.FindManyAsync(order.ProductIds, cancellationToken);
        return found.ToDictionary(p => p.Id);
    }

    private async Task<Order> RequireAsync(Guid id, CancellationToken cancellationToken)
    {
        return await orders.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), id);
    }
}
