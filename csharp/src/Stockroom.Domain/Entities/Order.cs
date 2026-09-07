using Stockroom.Domain.Exceptions;

namespace Stockroom.Domain.Entities;

public sealed class Order
{
    private readonly List<OrderLine> _lines = [];

    private Order()
    {
    }

    public Guid Id { get; private set; }

    public string CustomerEmail { get; private set; } = string.Empty;

    public OrderStatus Status { get; private set; }

    public DateTimeOffset PlacedAt { get; private set; }

    public DateTimeOffset? ShippedAt { get; private set; }

    public DateTimeOffset? CancelledAt { get; private set; }

    public IReadOnlyList<OrderLine> Lines => _lines;

    public decimal Total => _lines.Sum(l => l.LineTotal);

    public static Order Place(string customerEmail, IEnumerable<(Product Product, int Quantity)> items, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(customerEmail);
        ArgumentNullException.ThrowIfNull(items);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerEmail = customerEmail.Trim(),
            Status = OrderStatus.Pending,
            PlacedAt = now,
        };

        foreach (var (product, quantity) in items)
        {
            if (order._lines.Any(l => l.ProductId == product.Id))
            {
                throw new DomainException($"Product {product.Sku} appears more than once in the order.");
            }

            product.Reserve(quantity, now);
            order._lines.Add(OrderLine.Create(order.Id, product, quantity));
        }

        if (order._lines.Count == 0)
        {
            throw new DomainException("An order must contain at least one line.");
        }

        return order;
    }

    public void Ship(IReadOnlyDictionary<Guid, Product> products, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(products);
        EnsureStatus(OrderStatus.Pending, "shipped");

        foreach (var line in _lines)
        {
            ResolveProduct(products, line).Consume(line.Quantity, now);
        }

        Status = OrderStatus.Shipped;
        ShippedAt = now;
    }

    public void Cancel(IReadOnlyDictionary<Guid, Product> products, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(products);
        EnsureStatus(OrderStatus.Pending, "cancelled");

        foreach (var line in _lines)
        {
            ResolveProduct(products, line).Release(line.Quantity, now);
        }

        Status = OrderStatus.Cancelled;
        CancelledAt = now;
    }

    private static Product ResolveProduct(IReadOnlyDictionary<Guid, Product> products, OrderLine line)
    {
        if (!products.TryGetValue(line.ProductId, out var product))
        {
            throw new DomainException($"Product {line.Sku} referenced by the order was not supplied.");
        }

        return product;
    }

    private void EnsureStatus(OrderStatus expected, string action)
    {
        if (Status != expected)
        {
            throw new InvalidOrderStateException(Id, Status, action);
        }
    }
}
