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

    public decimal LineTotal => Quantity * UnitPrice;

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
