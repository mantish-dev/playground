using Stockroom.Domain.Entities;

namespace Stockroom.Application.Orders;

public sealed record OrderLineDto(Guid ProductId, string Sku, int Quantity, decimal UnitPrice, decimal LineTotal)
{
    public static OrderLineDto From(OrderLine line)
    {
        ArgumentNullException.ThrowIfNull(line);
        return new OrderLineDto(line.ProductId, line.Sku, line.Quantity, line.UnitPrice, line.LineTotal);
    }
}

public sealed record OrderDto(
    Guid Id,
    string CustomerEmail,
    OrderStatus Status,
    decimal Total,
    DateTimeOffset PlacedAt,
    DateTimeOffset? ShippedAt,
    DateTimeOffset? CancelledAt,
    IReadOnlyList<OrderLineDto> Lines)
{
    public static OrderDto From(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        return new OrderDto(
            order.Id,
            order.CustomerEmail,
            order.Status,
            order.Total,
            order.PlacedAt,
            order.ShippedAt,
            order.CancelledAt,
            order.Lines.Select(OrderLineDto.From).ToList());
    }
}

public sealed record PlaceOrderLineRequest(Guid ProductId, int Quantity);

public sealed record PlaceOrderRequest(string CustomerEmail, IReadOnlyList<PlaceOrderLineRequest> Lines);
