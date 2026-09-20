namespace Stockroom.Domain.Entities;

public sealed class OrderLine
{
    private OrderLine()
    {
    }

    public Guid Id { get; private set; }

    public Guid ProductId { get; private set; }

    public string Sku { get; private set; } = string.Empty;

    public int Quantity { get; private set; }

    public decimal UnitPrice { get; private set; }

    /// <summary>
    /// What this line contributes to the order total. Computed rather than stored: quantity and
    /// unit price are both frozen at placement, so the product of the two cannot drift.
    /// </summary>
    public decimal LineTotal => Quantity * UnitPrice;

    /// <summary>
    /// Builds a line from the product it reserves, copying the SKU and the unit price as they
    /// stand now — a later rename or repricing must not rewrite an order already placed. The
    /// owning order holds the line; the line no longer carries the order's id.
    /// </summary>
    /// <param name="product">The product being ordered.</param>
    /// <param name="quantity">Units ordered. Must be positive.</param>
    internal static OrderLine Create(Product product, int quantity)
    {
        ArgumentNullException.ThrowIfNull(product);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        return new OrderLine
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Sku = product.Sku,
            Quantity = quantity,
            UnitPrice = product.UnitPrice,
        };
    }
}
