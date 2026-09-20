using Microsoft.Extensions.Logging;
using Stockroom.Business.Abstractions;
using Stockroom.Business.Exceptions;
using Stockroom.Business.Orders;
using Stockroom.Domain.Entities;

namespace Stockroom.Application.Orders;

public sealed class OrderService(
    IOrderRepository orders,
    IOrderWorkflow workflow,
    IUnitOfWork unitOfWork,
    ILogger<OrderService> logger) : IOrderService
{
    public async Task<IReadOnlyList<OrderDto>> ListAsync(OrderStatus? status, CancellationToken cancellationToken)
    {
        var items = await orders.ListAsync(status, cancellationToken);
        return items.Select(OrderDto.From).ToList();
    }

    public async Task<OrderDto> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await GetRequiredAsync(id, cancellationToken);
        return OrderDto.From(order);
    }

    public async Task<OrderDto> PlaceAsync(PlaceOrderRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        PlaceOrderRequestValidator.Validate(request);

        var lines = request.Lines.Select(l => new OrderRequestLine(l.ProductId, l.Quantity)).ToList();
        var order = await workflow.PlaceAsync(request.CustomerEmail, lines, cancellationToken);

        orders.Add(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.OrderPlaced(order.Id, order.CustomerEmail, order.Lines.Count, order.Total);
        return OrderDto.From(order);
    }

    public async Task<OrderDto> ShipAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await GetRequiredAsync(id, cancellationToken);
        await workflow.ShipAsync(order, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.OrderShipped(order.Id);
        return OrderDto.From(order);
    }

    public async Task<OrderDto> CancelAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await GetRequiredAsync(id, cancellationToken);
        await workflow.CancelAsync(order, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.OrderCancelled(order.Id);
        return OrderDto.From(order);
    }

    private async Task<Order> GetRequiredAsync(Guid id, CancellationToken cancellationToken)
    {
        return await orders.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), id);
    }
}
